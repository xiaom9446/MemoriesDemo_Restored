using System;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace MemoriesDemo.Views
{
    public sealed partial class AlbumItemSmallView : UserControl
    {
        private AlbumVM _albumVM;
        private Canvas _elementOutsidePopup;

        public AlbumItemSmallView()
        {
            this.InitializeComponent();

            // 半透明遮罩
            this._elementOutsidePopup = new Canvas();
            this._elementOutsidePopup.Background = new SolidColorBrush(Colors.Transparent);
            this._elementOutsidePopup.Opacity = 0.4;
            this._elementOutsidePopup.Width = 1476.0;
            this._elementOutsidePopup.Height = 900.0;
            this._elementOutsidePopup.PointerPressed += OnElementOutsidePopup_PointerPressed;

            var canvas = new Canvas();
            this._popup.Child = canvas;
            canvas.Children.Add(this._elementOutsidePopup);
            canvas.Children.Add(this._popupChild);

            this._btn_Delete.Click += OnBtn_Delete_Click;

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Init(base.DataContext as AlbumVM);
        }

        private void Init(AlbumVM albumVM)
        {
            this._albumVM = albumVM;

            this.RightTapped += OnAlbumItemSmallView_RightTapped;
            this.Holding    += OnAlbumItemSmallView_Holding;

            if (!string.IsNullOrEmpty(this._albumVM.Image))
            {
                var image = new Image { Stretch = Stretch.UniformToFill };
                var bmp = new BitmapImage
                {
                    DecodePixelHeight = 300,
                    DecodePixelWidth  = 300,
                    UriSource = new Uri(MainPage._s_Instance.BaseUri, this._albumVM.Image)
                };
                image.Source = bmp;

                this._panel_Media.Children.Clear();
                this._panel_Media.Children.Add(image);
            }
        }

        private void OnAlbumItemSmallView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            this.ShowPopup(e.GetPosition(this));
        }

        private void OnAlbumItemSmallView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            this.ShowPopup(e.GetPosition(this));
        }

        private void ShowPopup(Point pos)
        {
            this._popup.IsOpen = true;

            var transform = this.TransformToVisual(MainPage._s_Instance.LayoutRoot);
            Point origin = transform.TransformPoint(default(Point));
            this._elementOutsidePopup.RenderTransform = new CompositeTransform
            {
                TranslateX = -origin.X,
                TranslateY = -origin.Y
            };

            var innerTransform = this._popupChild.TransformToVisual(this._panel_Media);
            Point innerPos = innerTransform.TransformPoint(default(Point));
            this._popupChild.RenderTransform = new CompositeTransform
            {
                TranslateX = -innerPos.X,
                TranslateY = -innerPos.Y
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

        private void OnBtn_Delete_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this._albumVM.OnDeletingAlbum();
            MainViewModel.Instance.Albums.Remove(this._albumVM);
        }
    }
}