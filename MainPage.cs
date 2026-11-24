using System;
using System.Collections.Generic;
using System.Linq;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using MemoriesDemo.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Search;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace MemoriesDemo
{
    public sealed partial class MainPage : UserControl
    {
        public static MainPage _s_Instance { get; private set; }
        public bool NavigationInProgress { get; set; }
        private bool _mediaStopNavigationPending;
        public bool _isBandLayoutSizeSet { get; set; }
        public double _bandHeight { get; set; }

        // WinRT XAML 不支持 x:FieldModifier，生成的字段默认是 internal。
        // 这里用公开属性把根 Grid 暴露出去，供 DialogHelper / 各 View 访问。
        public Grid LayoutRoot
        {
            get { return this.RootGrid; }
        }

        private SearchPane _searchPane;
        private DataTransferManager _share;
        private Storyboard _timer_SnappedView;
        private Random _random;
        private bool? _isSnappedCache = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.Init();
        }

        private async void Init()
        {
            _s_Instance = this;
            this._isBandLayoutSizeSet = false;

            // ---- 1. 载入本地已保存的相册 ----
            var albums = await DataServices.GetAlbumsFromStorageAsync();
            if (albums != null)
            {
                foreach (var album in albums)
                {
                    MainViewModel.Instance.Albums.Add(album);
                }
            }

            // ---- 2. 屏幕方向 / 视图状态（Snapped 分屏）----
            // Win8.0 的 ApplicationView 是静态类型，既没有 GetForCurrentView()，
            // 也不能作为事件处理器的参数类型；改用 SizeChanged 判断分屏
            // （Win8 的 Snapped 宽度固定为 320，取 500 作为阈值）。
            DisplayProperties.AutoRotationPreferences = DisplayOrientations.Landscape;
            this.SizeChanged += OnMainPage_SizeChanged;

            // ---- 3. 搜索面板 ----
            this._searchPane = SearchPane.GetForCurrentView();
            this._searchPane.PlaceholderText = "Search for album";
            this._searchPane.QuerySubmitted += _searchPane_QuerySubmitted;
            this._searchPane.SuggestionsRequested += _searchPane_SuggestionsRequested;

            // ---- 4. 分享 ----
            this._share = DataTransferManager.GetForCurrentView();
            this._share.DataRequested += _share_DataRequested;

            // ---- 5. 首页按钮 / 导航钩子 ----
            this._btn_Home.Click += OnBtn_Home_Click;
            this._frame.Navigating += OnFrame_Navigating;

            // ---- 6. Snapped 视图轮播计时器 ----
            this._timer_SnappedView = new Storyboard();
            this._timer_SnappedView.BeginTime = TimeSpan.FromMilliseconds(1500);
            this._timer_SnappedView.Completed += OnTimer_Completed;
            this._random = new Random((int)DateTime.Now.Ticks);

            this._listView_Snapped.SelectionMode = ListViewSelectionMode.None;
            this._listView_Snapped.CanDragItems = false;
            this._listView_Snapped.CanReorderItems = false;
            this._listView_Snapped.IsItemClickEnabled = false;
            this._listView_Snapped.ItemsSource = MainViewModel.Instance.Albums;

            NavigationManager.Instance.RootPanel = this._subLayoutRoot;
            VisualStateManager.GoToState(this, "NormalView", false);

            // ---- 7. 进入首页 ----
            this._frame.Navigate(typeof(AlbumsView));
        }

        public void NavigateToSearchPage(string queryText)
        {
            this._frame.Navigate(typeof(SearchResultsView), queryText);
        }

        // ================= 分享 =================
        private async void _share_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var media = MainViewModel.Instance.CurrentMediaItemVM;
            if (media == null)
            {
                args.Request.FailWithDisplayText("Please click on a photo");
                return;
            }

            var deferral = args.Request.GetDeferral();
            try
            {
                StorageFile file;
                if (!media.IsMediaFromStream)
                {
                    // WinRT 的 FrameworkElement 没有 BaseUri，这里显式补全 ms-appx 前缀
                    string uri = media.MediaSource;
                    if (!uri.StartsWith("ms-appx", StringComparison.OrdinalIgnoreCase))
                    {
                        uri = "ms-appx:///" + uri.TrimStart('/');
                    }
                    file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
                }
                else
                {
                    file = await StorageFile.GetFileFromPathAsync(media.MediaSource);
                }

                // Win8.0 的 DataPackage.SetBitmap 只接受 RandomAccessStreamReference，
                // 不能直接传 IRandomAccessStream
                args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(file));
                args.Request.Data.Properties.Title = "Memories Photo";
                args.Request.Data.Properties.Description = "Memories Photo Description";
            }
            catch (Exception)
            {
                args.Request.FailWithDisplayText("Unable to share this photo");
            }
            finally
            {
                deferral.Complete();
            }
        }

        // ================= 搜索 =================
        private void _searchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            var titles = new List<string>();
            foreach (var item in MainViewModel.Instance.Albums)
            {
                var album = item as AlbumVM;
                if (album != null && !string.IsNullOrEmpty(album.Title))
                {
                    titles.Add(album.Title);
                }
            }
            var matched = titles.Where(t => t.ContainsString(args.QueryText)).ToList();
            args.Request.SearchSuggestionCollection.AppendQuerySuggestions(matched);
        }

        private void _searchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            this._frame.Navigate(typeof(SearchResultsView), args.QueryText);
        }

        // ================= 视图状态 / 布局 =================
        private void OnMainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ApplyViewState(e.NewSize.Width < 500);
        }

        private void ApplyViewState(bool isSnapped)
        {
            if (this._isSnappedCache == isSnapped) return;
            this._isSnappedCache = isSnapped;

            if (isSnapped)
            {
                // 进入分屏：停掉主 Frame，改成小图轮播
                this._frame.IsEnabled = false;
                foreach (var item in MainViewModel.Instance.Albums)
                {
                    var album = item as AlbumVM;
                    if (album != null)
                    {
                        album.DisplayAnimationCompleted += OnAlbumsView_DisplayAnimationCompleted;
                    }
                }
                this._timer_SnappedView.Begin();
                this._listView_Snapped.IsEnabled = true;
                VisualStateManager.GoToState(this, "Snapped", true);
            }
            else
            {
                // 回到全屏
                this._frame.IsEnabled = true;
                foreach (var item in MainViewModel.Instance.Albums)
                {
                    var album = item as AlbumVM;
                    if (album == null) continue;
                    album.DisableDisplayAnimations = false;
                    album.DisplayAnimationCompleted -= OnAlbumsView_DisplayAnimationCompleted;
                }
                this._timer_SnappedView.Stop();
                this._listView_Snapped.IsEnabled = false;
                VisualStateManager.GoToState(this, "NormalView", true);
                this._frame.UpdateLayout();
            }
        }

        private void OnTimer_Completed(object sender, object e)
        {
            int count = MainViewModel.Instance.Albums.Count;
            if (count < 2) return;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                int index = this._random.Next(0, count - 1);
                var container = this._listView_Snapped.ItemContainerGenerator
                    .ContainerFromIndex(index) as FrameworkElement;
                if (container == null) continue;

                var album = container.DataContext as AlbumVM;
                if (album != null)
                {
                    album.TriggerDisplayAnimations = !album.TriggerDisplayAnimations;
                    return;
                }
            }
        }

        private void OnAlbumsView_DisplayAnimationCompleted(object sender, EventArgs e)
        {
            this._timer_SnappedView.Begin();
        }

        // ================= 导航 =================
        private void OnFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (this.NavigationInProgress)
            {
                e.Cancel = true;
                return;
            }

            if (e.SourcePageType == typeof(AlbumsView))
            {
                this.DoAlbumsViewTransition();
                return;
            }

            if (e.SourcePageType == typeof(AlbumDetailsView))
            {
                return;
            }

            if (e.SourcePageType == typeof(CreateAlbumView))
            {
                VisualStateManager.GoToState(this, "CreateAlbumView", true);
            }
        }

        private async void OnBtn_GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (!this._frame.CanGoBack || !this._frame.IsEnabled || this.NavigationInProgress) return;
            if (this._mediaStopNavigationPending) return;

            // 先在当前页面里把视频停稳，再让 Frame 拆页面。
            // 顺序反过来的话，页面销毁时媒体管线还活着，XAML 会抛
            // stowed exception（0xC00001AD / Windows.UI.Xaml.dll），托管层接不住。
            await this.StopMediaBeforeLeavingAsync();

            if (this._frame.CanGoBack && this._frame.IsEnabled && !this.NavigationInProgress)
            {
                this._frame.GoBack();
            }
        }

        private async void OnBtn_Home_Click(object sender, RoutedEventArgs e)
        {
            if (!this._frame.IsEnabled || this.NavigationInProgress) return;
            if (this._mediaStopNavigationPending) return;

            await this.StopMediaBeforeLeavingAsync();

            if (this._frame.IsEnabled && !this.NavigationInProgress)
            {
                this._frame.Navigate(typeof(AlbumsView));
            }
        }

        /// <summary>
        /// 离开相册详情页前先停播。当前页不是详情页、或页面里没有活跃视频时，
        /// StopMediaAndWaitAsync 会立刻返回，不会给普通返回增加延迟。
        /// </summary>
        private System.Threading.Tasks.Task StopMediaBeforeLeavingAsync()
        {
            // 【复现专用】还原 patch19 行为：导航前不做任何停播处理。
            // 保留方法名，调用点的 await 不用改。
            return System.Threading.Tasks.Task.FromResult<object>(null);
        }

        // ================= 调试信息 =================
        public static void ShowDebugMessage(string msg)
        {
            if (MainPage._s_Instance == null || MainPage._s_Instance._tb_DEBUG == null) return;
            MainPage._s_Instance._tb_DEBUG.Visibility = Visibility.Visible;
            MainPage._s_Instance._tb_DEBUG.Text = msg;
        }

        // ================= 顶部 band 的转场动画 =================
        public static void StartAlbumDetailsViewTransitionAnimation()
        {
            if (MainPage._s_Instance != null)
            {
                MainPage._s_Instance.StartAlbumDetailsViewTransitionAnimationHelper();
            }
        }

        public static void UpdateBandLayoutSize(double height)
        {
            if (MainPage._s_Instance != null)
            {
                MainPage._s_Instance.UpdateBandLayoutSizeHelper(height);
            }
        }

        private void StartAlbumDetailsViewTransitionAnimationHelper()
        {
            this.DoAlbumDetailsViewTransition();
        }

        private void UpdateBandLayoutSizeHelper(double height)
        {
            if (!this._isBandLayoutSizeSet)
            {
                this._isBandLayoutSizeSet = true;
                this._bandHeight = height - 16.0;
                this._band.Height = this._bandHeight;
            }
        }

        /// <summary>进入相册详情：band 先上移收进顶部，再滑出屏幕</summary>
        private void DoAlbumDetailsViewTransition()
        {
            double height = Window.Current.Bounds.Height;
            double width = Window.Current.Bounds.Width;
            double bandTarget = -height + (this._band.Height - 30.0);

            var storyboard = new Storyboard();

            var moveY = new DoubleAnimationUsingKeyFrames();
            moveY.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(0),
                Value = 0
            });
            moveY.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(500),
                Value = bandTarget
            });
            moveY.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(750),
                Value = bandTarget,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });
            Storyboard.SetTarget(moveY, this._band);
            Storyboard.SetTargetProperty(moveY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            storyboard.Children.Add(moveY);

            var moveX = new DoubleAnimationUsingKeyFrames();
            moveX.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(0),
                Value = 0
            });
            moveX.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(1000),
                Value = 0
            });
            moveX.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(1500),
                Value = width + 20.0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });
            Storyboard.SetTarget(moveX, this._band);
            Storyboard.SetTargetProperty(moveX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            storyboard.Children.Add(moveX);

            storyboard.Begin();
        }

        /// <summary>返回相册列表：band 从屏幕外滑回原位</summary>
        private void DoAlbumsViewTransition()
        {
            double height = Window.Current.Bounds.Height;
            double width = Window.Current.Bounds.Width;
            double bandStart = -height + (this._band.Height - 30.0);

            var storyboard = new Storyboard();

            var moveY = new DoubleAnimationUsingKeyFrames();
            moveY.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(0),
                Value = bandStart
            });
            moveY.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(500),
                Value = bandStart
            });
            moveY.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(1000),
                Value = 0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });
            Storyboard.SetTarget(moveY, this._band);
            Storyboard.SetTargetProperty(moveY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            storyboard.Children.Add(moveY);

            var moveX = new DoubleAnimationUsingKeyFrames();
            moveX.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(0),
                Value = width + 20.0
            });
            moveX.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(500),
                Value = 0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });
            Storyboard.SetTarget(moveX, this._band);
            Storyboard.SetTargetProperty(moveX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            storyboard.Children.Add(moveX);

            storyboard.Begin();
        }
    }
}
