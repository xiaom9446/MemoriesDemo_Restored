using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using System.Reflection;

namespace MemoriesDemo.Views
{
    public sealed partial class AlbumsView : Page
    {
        private Storyboard _timer_DisplayAnimations;
        private Random _random;
        private bool _isBrowseView;
        private AlbumVM _albumToDelete;
        private IAlbumItemTemplateView _albumTemplateToDelete;
        private bool _isAppBarOpen;

        private Dictionary<string, object> _ctrlCache = new Dictionary<string, object>();

        private T Ctrl<T>(string name) where T : class
        {
            object cached;
            if (_ctrlCache.TryGetValue(name, out cached))
                return cached as T;
            var v = this.FindName(name) as T;
            _ctrlCache[name] = v;
            return v;
        }

        public AlbumsView()
        {
            this.InitializeComponent();

            var b = Ctrl<Button>("_btn_Browse");        if (b != null) b.Click += OnGridView_ViewChange_Click;
            b = Ctrl<Button>("_btn_Grid");              if (b != null) b.Click += OnGridView_ViewChange_Click;
            b = Ctrl<Button>("_btn_NewAlbum");          if (b != null) b.Click += OnBtn_NewAlbum_Click;
            b = Ctrl<Button>("_btn_GenerateAlbum");     if (b != null) b.Click += OnBtn_GenerateAlbum_Click;
            b = Ctrl<Button>("_btn_DeleteAlbum");       if (b != null) b.Click += OnBtn_DeleteAlbum_Click;

            // 两个列表都必须 IsItemClickEnabled=true，否则 ItemClick 永远不触发；
            // 同时把 SelectionMode 设为 None，否则点一下只是“打勾选中”而不是进相册。
            var gv1 = Ctrl<ListView>("_gridView1");
            if (gv1 != null)
            {
                gv1.IsItemClickEnabled = true;
                gv1.SelectionMode = ListViewSelectionMode.None;
                gv1.ItemClick += OnGridView1_ItemClick;
                gv1.ItemTemplateSelector = new AlbumItemTemplateSelector();
            }
            var gv2 = Ctrl<ListView>("_gridView2");
            if (gv2 != null)
            {
                gv2.IsItemClickEnabled = true;
                gv2.SelectionMode = ListViewSelectionMode.None;
                gv2.ItemClick += OnGridView1_ItemClick;
            }

            // 右键（或长按）空白处 → 弹出 AppBar
            this.RightTapped += OnAlbumsView_RightTapped;

            this._random = new Random((int)DateTime.Now.Ticks);
            this.IsEnabledChanged += OnAlbumsView_IsEnabledChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (MainPage._s_Instance != null)
                MainPage._s_Instance.NavigationInProgress = true;

            base.DataContext = MainViewModel.Instance;
            this._isBrowseView = true;
            MainViewModel.Instance.CurrentView = View.AlbumsView;
            MainViewModel.Instance.CurrentAlbum = null;
            MainViewModel.Instance.CurrentMediaItemVM = null;
            MainViewModel.Instance.CurrentPage = null;

            var ph = Ctrl<Grid>("_gridPlaceholder");
            if (ph != null) ph.MinWidth = Window.Current.Bounds.Width;

            if (MainPage._s_Instance != null && MainPage._s_Instance._isBandLayoutSizeSet)
            {
                var rf1 = Ctrl<Rectangle>("_rectFade1");
                if (rf1 != null) rf1.Height = MainPage._s_Instance._bandHeight;
                var rf2 = Ctrl<Rectangle>("_rectFade2");
                if (rf2 != null) rf2.Height = MainPage._s_Instance._bandHeight;
            }

            foreach (AlbumVM album in MainViewModel.Instance.Albums)
                album.ShowAlbumMenu += OnAlbumsView_ShowAlbumMenu;

            var gv1 = Ctrl<ListView>("_gridView1");
            if (gv1 != null) gv1.ItemsSource = MainViewModel.Instance.Albums;
            var gv2 = Ctrl<ListView>("_gridView2");
            if (gv2 != null) gv2.ItemsSource = MainViewModel.Instance.Albums;

            var sb = Ctrl<Storyboard>("sb_TransitionIn");
            if (sb != null)
            {
                sb.Completed += (s, ev) =>
                {
                    if (MainPage._s_Instance != null)
                        MainPage._s_Instance.NavigationInProgress = false;
                    this.StartDisplayAnimations();
                };
                sb.Begin();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.StopDisplayAnimations();
            this.SetAppBar(false);
            foreach (AlbumVM album in MainViewModel.Instance.Albums)
            {
                album.DisplayAnimationCompleted -= OnAlbumsView_DisplayAnimationCompleted;
                album.ShowAlbumMenu -= OnAlbumsView_ShowAlbumMenu;
            }
            var sb = Ctrl<Storyboard>("sb_TransitionOut");
            if (sb != null) sb.Begin();
        }

        private void OnAlbumsView_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsEnabled) this.StopDisplayAnimations();
        }

        private void OnGridView1_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (MainPage._s_Instance != null && MainPage._s_Instance.NavigationInProgress) return;
            AlbumVM album = e.ClickedItem as AlbumVM;
            if (album != null)
            {
                MainViewModel.Instance.CurrentAlbum = album;
                MainViewModel.Instance.IsNewAlbum = false;
                this.TransitionOutGridView(null);
            }
        }

        private void OnBtn_GenerateAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (MainPage._s_Instance != null && MainPage._s_Instance.NavigationInProgress) return;
            this.ExecuteNewAlbum(false);
        }

        private void OnBtn_NewAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (MainPage._s_Instance != null && MainPage._s_Instance.NavigationInProgress) return;
            this.ExecuteNewAlbum(true);
        }

        private void ExecuteNewAlbum(bool isCreateNew)
        {
            MainPage.StartAlbumDetailsViewTransitionAnimation();
            var album = new AlbumVM { Title = "My New Album", IsEditing = true };
            MainViewModel.Instance.CurrentAlbum = album;
            MainViewModel.Instance.Albums.Add(album);
            if (isCreateNew) album.IsNewAlbum = true;
            else album.IsGenerated = true;
            this.TransitionOutGridView(null);
        }

        private void OnTimer_Completed(object sender, object e)
        {
            if (this._timer_DisplayAnimations != null)
                this._timer_DisplayAnimations.Completed -= OnTimer_Completed;
            bool found = false;
            int guard = 0;
            while (!found && MainViewModel.Instance.CurrentView == View.AlbumsView && guard++ < 100)
            {
                if (MainViewModel.Instance.Albums.Count <= 1) break;
                int idx = this._random.Next(0, MainViewModel.Instance.Albums.Count - 1);
                var gv1 = Ctrl<ListView>("_gridView1");
                FrameworkElement container = null;
                if (gv1 != null)
                    container = gv1.ItemContainerGenerator.ContainerFromIndex(idx) as FrameworkElement;
                if (container != null)
                {
                    var album = container.DataContext as AlbumVM;
                    if (album != null && album.IsDemoAlbum)
                    {
                        album.DisplayAnimationCompleted += OnAlbumsView_DisplayAnimationCompleted;
                        album.TriggerDisplayAnimations = !album.TriggerDisplayAnimations;
                        found = true;
                    }
                }
            }
        }

        private void OnAlbumsView_DisplayAnimationCompleted(object sender, EventArgs e)
        {
            var album = sender as AlbumVM;
            if (album != null)
                album.DisplayAnimationCompleted -= OnAlbumsView_DisplayAnimationCompleted;
            if (this._timer_DisplayAnimations != null)
                this.StartDisplayAnimations();
        }

        private void StartDisplayAnimations() { }
        private void StopDisplayAnimations() { }

        private void OnGridView_ViewChange_Click(object sender, RoutedEventArgs e)
        {
            bool isBrowse = sender == Ctrl<Button>("_btn_Browse");
            if (this._isBrowseView != isBrowse)
            {
                this._isBrowseView = isBrowse;
                VisualStateManager.GoToState(this, isBrowse ? "Browse" : "Grid", true);
            }
            this.HideAlbumMenu();
        }

        private void OnAlbumsView_ShowAlbumMenu(object sender, ShowAlbumEventArgs e)
        {
            this.StopDisplayAnimations();
            var gv1 = Ctrl<ListView>("_gridView1"); if (gv1 != null) gv1.IsEnabled = false;
            var gv2 = Ctrl<ListView>("_gridView2"); if (gv2 != null) gv2.IsEnabled = false;
            var pam = Ctrl<StackPanel>("_panel_AlbumMenu");
            if (pam != null) pam.Visibility = Visibility.Visible;
            var pmm = Ctrl<StackPanel>("_panel_MainMenu");
            if (pmm != null) pmm.Visibility = Visibility.Collapsed;
            SetAppBar(true);
            this._albumToDelete = sender as AlbumVM;
            this._albumTemplateToDelete = e.AlbumItemTemplateView;
        }

        /// <summary>
        /// Win8 的 ApplicationBar 在 Win10 上完全不显示，所以这里用普通 Grid 自绘一个
        /// 底部 AppBar：默认藏在屏幕下方，SetAppBar(true) 时滑出。
        /// </summary>
        private void SetAppBar(bool isOpen)
        {
            var bar = Ctrl<Grid>("_appbar");
            if (bar == null) return;

            var scrim = Ctrl<Grid>("_appbarScrim");
            var tf = bar.RenderTransform as CompositeTransform;
            double h = bar.Height > 0.0 ? bar.Height : 96.0;

            if (isOpen)
            {
                bar.Visibility = Visibility.Visible;
                if (scrim != null) scrim.Visibility = Visibility.Visible;
                this._isAppBarOpen = true;

                if (tf == null) return;
                var sb = new Storyboard();
                var slide = new DoubleAnimation
                {
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(220)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(slide, tf);
                Storyboard.SetTargetProperty(slide, "TranslateY");
                sb.Children.Add(slide);
                sb.Begin();
            }
            else
            {
                if (scrim != null) scrim.Visibility = Visibility.Collapsed;
                this._isAppBarOpen = false;

                if (tf == null)
                {
                    bar.Visibility = Visibility.Collapsed;
                    return;
                }
                var sb = new Storyboard();
                var slide = new DoubleAnimation
                {
                    To = h,
                    Duration = new Duration(TimeSpan.FromMilliseconds(160)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                Storyboard.SetTarget(slide, tf);
                Storyboard.SetTargetProperty(slide, "TranslateY");
                sb.Children.Add(slide);
                sb.Completed += (s, e) => { bar.Visibility = Visibility.Collapsed; };
                sb.Begin();
            }
        }

        /// <summary>右键空白处：弹出默认菜单（List / Grid / New Album / Generate Album）</summary>
        private void OnAlbumsView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // 右键点在相册卡片上时，卡片自己已经把 _albumToDelete 设好了，交给专辑菜单
            if (this._albumToDelete != null) return;

            var pmm = Ctrl<StackPanel>("_panel_MainMenu");
            if (pmm != null) pmm.Visibility = Visibility.Visible;
            var pam = Ctrl<StackPanel>("_panel_AlbumMenu");
            if (pam != null) pam.Visibility = Visibility.Collapsed;

            this.StopDisplayAnimations();
            SetAppBar(true);
        }

        /// <summary>点 AppBar 外的遮罩：收起</summary>
        private void OnAppBarScrim_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.HideAlbumMenu();
        }

        private void OnBtn_DeleteAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (this._albumToDelete != null)
            {
                this._albumToDelete.DisplayAnimationCompleted -= OnAlbumsView_DisplayAnimationCompleted;
                this._albumToDelete.ShowAlbumMenu -= OnAlbumsView_ShowAlbumMenu;
                this._albumToDelete.OnDeletingAlbum();
                MainViewModel.Instance.Albums.Remove(this._albumToDelete);
            }
            this.HideAlbumMenu();
        }

        private void HideAlbumMenu()
        {
            var gv1 = Ctrl<ListView>("_gridView1"); if (gv1 != null) gv1.IsEnabled = true;
            var gv2 = Ctrl<ListView>("_gridView2"); if (gv2 != null) gv2.IsEnabled = true;
            this._albumToDelete = null;
            this._albumTemplateToDelete = null;
            var pam = Ctrl<StackPanel>("_panel_AlbumMenu");
            if (pam != null) pam.Visibility = Visibility.Collapsed;
            var pmm = Ctrl<StackPanel>("_panel_MainMenu");
            if (pmm != null) pmm.Visibility = Visibility.Visible;
            SetAppBar(false);
            this.StartDisplayAnimations();
        }

        private void AlbumItemInner_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0.0 &&
                MainPage._s_Instance != null &&
                !MainPage._s_Instance._isBandLayoutSizeSet)
            {
                var fe = sender as FrameworkElement;
                if (fe != null)
                {
                    try
                    {
                        var transform = fe.TransformToVisual(MainPage._s_Instance.LayoutRoot);
                        Point pos = transform.TransformPoint(default(Point));
                        double remaining = Window.Current.Bounds.Height - pos.Y;
                        if (remaining > 0.0)
                        {
                            MainPage.UpdateBandLayoutSize(remaining);
                            var rf1 = Ctrl<Rectangle>("_rectFade1");
                            if (rf1 != null) rf1.Height = MainPage._s_Instance._bandHeight;
                            var rf2 = Ctrl<Rectangle>("_rectFade2");
                            if (rf2 != null) rf2.Height = MainPage._s_Instance._bandHeight;
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        private void TransitionOutGridView(AlbumVM skipItem)
        {
            this.StopDisplayAnimations();
            var sb = new Storyboard();
            sb.Completed += (s, e) =>
            {
                if (base.Frame != null)
                    base.Frame.Navigate(typeof(AlbumDetailsView));
            };

            double delay = 0.0;
            var duration = new Duration(TimeSpan.FromMilliseconds(100.0));
            int count = Math.Min(6, MainViewModel.Instance.Albums.Count);

            var gv = Ctrl<ListView>("_gridView1");
            for (int i = 0; i < count; i++)
            {
                ListViewItem item = null;
                if (gv != null) item = gv.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (item == null) continue;
                var album = item.DataContext as AlbumVM;
                if (album == skipItem) continue;
                if (!(item.RenderTransform is CompositeTransform))
                    item.RenderTransform = new CompositeTransform { TranslateX = 0.0, TranslateY = 0.0 };
                var slide = new DoubleAnimation
                {
                    From = 0.0, To = 1000.0, Duration = duration,
                    BeginTime = TimeSpan.FromMilliseconds(delay),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                Storyboard.SetTarget(slide, item);
                Storyboard.SetTargetProperty(slide, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                sb.Children.Add(slide);
                var fade = new DoubleAnimation
                {
                    From = 1.0, To = 0.0, Duration = duration,
                    BeginTime = TimeSpan.FromMilliseconds(delay),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(fade, item);
                Storyboard.SetTargetProperty(fade, "(UIElement.Opacity)");
                sb.Children.Add(fade);
                delay += 100.0;
            }

            var ph = Ctrl<Grid>("_panel_Header");
            if (ph != null)
            {
                var headerFade = new DoubleAnimation
                {
                    From = 1.0, To = 0.0, Duration = duration,
                    BeginTime = TimeSpan.FromMilliseconds(100.0),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(headerFade, ph);
                Storyboard.SetTargetProperty(headerFade, "(UIElement.Opacity)");
                sb.Children.Add(headerFade);
            }

            var sp = Ctrl<Grid>("_subPanel1");
            if (sp != null)
            {
                var subFade = new DoubleAnimation
                {
                    From = 1.0, To = 0.0, Duration = duration,
                    BeginTime = TimeSpan.FromMilliseconds(100.0),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(subFade, sp);
                Storyboard.SetTargetProperty(subFade, "(UIElement.Opacity)");
                sb.Children.Add(subFade);
            }

            sb.Begin();
            MainPage.StartAlbumDetailsViewTransitionAnimation();
        }
    }
}