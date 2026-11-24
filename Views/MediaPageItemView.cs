using System;
using System.Threading.Tasks;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI;

namespace MemoriesDemo.Views
{
    public sealed partial class MediaPageItemView : UserControl
    {
        private MediaItemVM _mediaItemVM;
        private Canvas _elementOutsidePopup;
        private bool _isEditMode;
        private IGenericAlbumPageView _parentView;
        private CoreDispatcher _cd;
        private bool _useCustomPopupSystem;

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(MediaPageItemView),
                new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MediaPageItemView).OnIsReadOnlyPropertyChanged(e);
        }

        private void OnIsReadOnlyPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isReadOnly = (bool)e.NewValue;
            this._tb_AddPhoto.Visibility = isReadOnly ? Visibility.Collapsed : Visibility.Visible;
        }

        public MediaItemVM MediaItemVM
        {
            get { return this._mediaItemVM; }
        }

        public IGenericAlbumPageView ParentView
        {
            get { return this._parentView; }
        }

        public MediaPageItemView()
        {
            this.InitializeComponent();

            this._cd = Window.Current.Dispatcher;

            this._btn_Edit.Click           += OnBtn_Edit_Click;
            this._btn_Delete.Click         += OnBtn_Delete_Click;
            this._btn_Capture_Photo.Click  += OnBtn_Capture_Photo_Click;
            this._btn_Add.Click            += OnBtn_Add_Click;
            this._btn_Change.Click         += OnBtn_Change_Click;

            this._elementOutsidePopup = new Canvas
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Opacity = 0.8,
                Width = 1476.0,
                Height = 900.0
            };
            this._elementOutsidePopup.PointerPressed += OnElementOutsidePopup_PointerPressed;

            var canvas = new Canvas();
            this._popup.Child = canvas;
            canvas.Children.Add(this._elementOutsidePopup);
            canvas.Children.Add(this._popupChild);

            this._panel_Media.RenderTransform = new CompositeTransform();
            this._useCustomPopupSystem = false;
        }

        public void Init(IGenericAlbumPageView albumPageView, MediaItemVM mediaItemVM, double width, double height)
        {
            this._parentView = albumPageView;
            this.LayoutRoot.Width = width;
            this.LayoutRoot.Height = height;
            this.IsReadOnly = mediaItemVM.IsReadOnly;
            this.SetupMediaView(mediaItemVM);

            this.Loaded    += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.RightTapped += OnMediaPageItemView_RightTapped;
            this.Tapped      += OnMediaPageItemView_Tapped;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.LayoutRoot.ActualWidth > 0.0)
            {
                this.LayoutRoot.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0.0, 0.0, this.LayoutRoot.ActualWidth, this.LayoutRoot.ActualHeight)
                };
            }
        }

        private void SetupMediaView(MediaItemVM mediaItemVM)
        {
            this._mediaItemVM = mediaItemVM;
            base.DataContext = this._mediaItemVM;
            VisualStateManager.GoToState(this, "Unselected", false);

            if (Helper.IsMediaTypeOfVideo(this._mediaItemVM.MediaSource))
                this._panel_PlayMedia.Visibility = Visibility.Visible;

            if (this._mediaItemVM.CompositeTransform != null)
                this._panel_Media.RenderTransform = Helper.CompositeTransformFromCTData(this._mediaItemVM.CompositeTransform);

            this.UpdateMenuState(true, false);

            if (MainViewModel.Instance.CurrentAlbum.IsEditing)
            {
                this.BeginEdit();
                if (string.IsNullOrEmpty(this._mediaItemVM.MediaSource))
                    this.UpdateMenuState(false, true);
            }

            if (!string.IsNullOrEmpty(this._mediaItemVM.MediaSource))
                this.UpdateMedia();
        }

        public void BeginEdit()
        {
            this._isEditMode = true;

            if (MainViewModel.Instance.CurrentView == View.EditPageView)
            {
                this.ManipulationMode = ManipulationModes.All;
                // 编辑态会被多次下发（进入编辑、滚动后补），这里必须先退订再订阅，
                // 否则同一个 handler 挂多份，拖一下图片会跳好几倍。
                this.ManipulationDelta -= OnManipulationDelta;
                this.ManipulationDelta += OnManipulationDelta;
            }

            this._panel_PlayMedia.Visibility = Visibility.Collapsed;

            if (this._mediaItemVM == null || string.IsNullOrEmpty(this._mediaItemVM.MediaSource))
            {
                this._panel_AddPhoto.Visibility = Visibility.Visible;
                return;
            }
            this._panel_AddPhoto.Visibility = Visibility.Collapsed;
        }

        public void CompleteEdit()
        {
            this._isEditMode = false;

            if (MainViewModel.Instance.CurrentView == View.EditPageView)
            {
                this.ManipulationMode = ManipulationModes.None;
                this.ManipulationDelta -= OnManipulationDelta;

                if (this._mediaItemVM != null)
                    this._mediaItemVM.CompositeTransform =
                        Helper.CTDataFromCompositeTransform(this._panel_Media.RenderTransform as CompositeTransform);
            }

            this._panel_AddPhoto.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(this._mediaItemVM.MediaSource))
            {
                this._panel_Media.Visibility = Visibility.Collapsed;
                this.UpdateMenuState(false, false);
                return;
            }
            this._panel_PlayMedia.Visibility =
                Helper.IsMediaTypeOfVideo(this._mediaItemVM.MediaSource) ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdateSelection(bool isSelected)
        {
            VisualStateManager.GoToState(this, isSelected ? "Selected" : "Unselected", true);
        }

        private void OnMediaPageItemView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.OnSelectedMediaItem(e.GetPosition(this));
        }

        private void OnMediaPageItemView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // 阻止继续冒泡：否则 AlbumDetailsView 的右键处理器会立刻把刚弹出的 AppBar 收起来
            e.Handled = true;
            this.OnSelectedMediaItem(e.GetPosition(this));
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!this._isEditMode) return;

            var ct = this._panel_Media.RenderTransform as CompositeTransform;

            ct.TranslateX += e.Delta.Translation.X;
            ct.TranslateY += e.Delta.Translation.Y;

            if (e.Delta.Scale != 1.0)
            {
                if (e.Cumulative.Scale > 1.0)
                {
                    double step = 0.025;
                    ct.ScaleX += step * e.Cumulative.Scale;
                    ct.ScaleY += step * e.Cumulative.Scale;
                    ct.ScaleX = Math.Min(ct.ScaleX, 4.0);
                    ct.ScaleY = Math.Min(ct.ScaleY, 4.0);
                    return;
                }
                double shrink = 0.05;
                ct.ScaleX -= shrink * e.Cumulative.Scale;
                ct.ScaleY -= shrink * e.Cumulative.Scale;
                ct.ScaleX = Math.Max(ct.ScaleX, 0.7);
                ct.ScaleY = Math.Max(ct.ScaleY, 0.7);
            }
        }

        private void OnBtn_Capture_Photo_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this.CapturePhoto(null);
        }

        private void OnBtn_Edit_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this.OnEditMediaItem();
        }

        private void OnEditMediaDialogComplete(object sender, object e)
        {
            (sender as INavigationElement).TransitionOutComplete -= OnEditMediaDialogComplete;
            MainViewModel.Instance.CurrentView = View.AlbumDetailsView;

            if (MainViewModel.Instance.CurrentMediaItemVM != null)
                this.SetupMediaView(MainViewModel.Instance.CurrentMediaItemVM);
        }

        private void OnBtn_Delete_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this.OnDeleteMediaItem();
        }

        private void OnBtn_Change_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this.PickFile(null);
        }

        private void OnBtn_Add_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this.PickFile(null);
        }

        private void OnSelectedMediaItem(Point pos)
        {
            MainViewModel.Instance.CurrentMediaItemVM = this._mediaItemVM;
            MainViewModel.Instance.CurrentPage = this.ParentView.PageVM;

            if (!this.IsReadOnly && MainViewModel.Instance.CurrentView != View.EditMediaView)
            {
                if (this._isEditMode)
                {
                    if (this._useCustomPopupSystem)
                    {
                        this.ShowPopup(pos);
                        return;
                    }
                    this._parentView.ShowAppBarMediaMenuItems(this);
                    return;
                }
                this.ExecuteMedia();
            }
        }

        public void OnEditMediaItem()
        {
            if (!string.IsNullOrEmpty(this._mediaItemVM.MediaSource) &&
                MainViewModel.Instance.CurrentView != View.EditMediaView)
            {
                MainViewModel.Instance.CurrentMediaItemVM = this._mediaItemVM;
                MainViewModel.Instance.CurrentPage = this._parentView.PageVM;

                var editView = new EditMediaView();
                editView.TransitionOutComplete += OnEditMediaDialogComplete;
                DialogHelper.ShowDialog(editView);
            }
        }

        public void OnDeleteMediaItem()
        {
            this._panel_Media.Children.Clear();
            this.UpdateMenuState(false, true);
            this._mediaItemVM.MediaSource = null;
        }

        public async void CapturePhoto(Action<bool> onCompleted = null)
        {
            var camera = new CameraCaptureUI();

            StorageFile sf = await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (sf == null)
            {
                if (onCompleted != null) onCompleted(false);
                return;
            }

            // 相机写入的是临时目录，Win8 之后同样读不到 —— 先复制进应用本地文件夹
            StorageFile local = await CopyToLocalFolder(sf);
            if (local == null) local = sf;

            string path = local.Path;
            IRandomAccessStream fileStream = await local.OpenAsync(FileAccessMode.Read);

            await this._cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this._mediaItemVM.IsMediaFromStream = true;
                this._mediaItemVM.MediaSource = path;
                this.UpdateMenuState(true, false);
                this._panel_Media.Visibility = Visibility.Visible;
                this._panel_AddPhoto.Visibility = Visibility.Collapsed;
                MediaPageItemViewHelper.UpdateMedia(this._mediaItemVM, this._panel_Media, local, fileStream, null, null);
                if (onCompleted != null) onCompleted(true);
            });
        }

        public async void PickFile(Action<bool> onCompleted = null)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".mp4");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            StorageFile sf = await picker.PickSingleFileAsync();
            if (sf == null)
            {
                if (onCompleted != null) onCompleted(false);
                return;
            }

            // 关键：用户选的多半在图片库之外（桌面、下载目录），
            // 直接把 sf.Path 记下来，下一次重绘时 GetFileFromPathAsync 会失败、图片直接消失。
            // 所以先复制到应用本地文件夹，之后一直可读。
            StorageFile local = await CopyToLocalFolder(sf);
            if (local == null) local = sf;

            string path = local.Path;
            IRandomAccessStream fileStream = await local.OpenAsync(FileAccessMode.Read);

            await this._cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this._mediaItemVM.IsMediaFromStream = true;
                this._mediaItemVM.MediaSource = path;
                this.UpdateMenuState(true, false);
                this._panel_Media.Visibility = Visibility.Visible;
                this._panel_AddPhoto.Visibility = Visibility.Collapsed;
                MediaPageItemViewHelper.UpdateMedia(this._mediaItemVM, this._panel_Media, local, fileStream, null, null);
                if (onCompleted != null) onCompleted(true);
            });
        }

        /// <summary>
        /// 把外部文件复制进 LocalFolder。Win8 只保证应用自己的存储可再次按路径读取，
        /// 图片库/桌面/相机的临时文件都不保证，失败时返回 null 由调用方兜底。
        /// </summary>
        private static async Task<StorageFile> CopyToLocalFolder(StorageFile file)
        {
            try
            {
                return await file.CopyAsync(
                    ApplicationData.Current.LocalFolder,
                    file.Name,
                    NameCollisionOption.GenerateUniqueName);
            }
            catch
            {
                return null;
            }
        }

        private void UpdateMedia()
        {
            this._panel_Media.Visibility = Visibility.Visible;
            MediaPageItemViewHelper.UpdateMedia(this._mediaItemVM, this._panel_Media, null, null);
        }

        private void ExecuteMedia()
        {
            if (!Helper.IsMediaTypeOfVideo(this._mediaItemVM.MediaSource)) return;

            MediaElement mediaElement = Helper.GetVisualChild<MediaElement>(this._panel_Media, 0);
            if (mediaElement == null) return;

            this._mediaItemVM.IsPlaying = !this._mediaItemVM.IsPlaying;

            if (this._mediaItemVM.IsPlaying)
            {
                this._panel_PlayMedia.Visibility = Visibility.Collapsed;
                mediaElement.Play();
                return;
            }
            this._panel_PlayMedia.Visibility = Visibility.Visible;
            mediaElement.Stop();
        }

        private void UpdateMenuState(bool hasMedia, bool showWatermark)
        {
            if (hasMedia)
            {
                this._panel_Edit.Visibility = Visibility.Visible;
                this._panel_Add.Visibility  = Visibility.Collapsed;
            }
            else
            {
                this._panel_Edit.Visibility = Visibility.Collapsed;
                this._panel_Add.Visibility  = Visibility.Visible;
            }

            this._panel_AddPhoto.Visibility = showWatermark ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowPopup(Point pos)
        {
            if (!this._isEditMode || this.IsReadOnly) return;

            this._popup.IsOpen = true;

            var transform = base.TransformToVisual(MainPage._s_Instance.LayoutRoot);
            Point point = transform.TransformPoint(default(Point));
            this._elementOutsidePopup.RenderTransform = new CompositeTransform
            {
                TranslateX = -point.X,
                TranslateY = -point.Y
            };

            transform = this._popup.TransformToVisual(this._panel_Media);
            point = transform.TransformPoint(default(Point));
            this._popupChild.RenderTransform = new CompositeTransform
            {
                TranslateX = -point.X,
                TranslateY = -point.Y
            };

            VisualStateManager.GoToState(this, "Editing", true);
        }

        private void HidePopup()
        {
            this._popup.IsOpen = false;
            VisualStateManager.GoToState(this, "NotEditing", true);
        }

        private void OnElementOutsidePopup_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.HidePopup();
        }
    }
}