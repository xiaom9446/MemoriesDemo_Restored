using System;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace MemoriesDemo.Views
{
    public sealed partial class SceneUserControl : UserControl, INavigationElement
    {
        private string _image1Asset;
        private string _image2Asset;
        private bool _isInputCaptured;
        private PointerPoint _pointerPos;
        private bool _isEditEnabled;

        public event EventHandler TransitionInComplete;
        public event EventHandler TransitionOutComplete;

        public SceneData SceneData { get; set; }

        public SceneUserControl()
        {
            this.InitializeComponent();

            this._isEditEnabled = false;

            this._image1.PointerPressed  += _panelEditCanvas_PointerPressed;
            this._image1.PointerMoved    += _panelEditCanvas_PointerMoved;
            this._image1.PointerReleased += _panelEditCanvas_PointerReleased;
        }

        public string GetUriString()
        {
            return "SceneUserControl.xaml";
        }

        public void TransitionIn()
        {
            EventHandler<object> onCompleted = null;
            onCompleted = (s, e) =>
            {
                this.sb_TransitionIn.Completed -= onCompleted;
                var handler = this.TransitionInComplete;
                if (handler != null) handler(this, null);
            };
            this.sb_TransitionIn.Completed += onCompleted;
            this.sb_TransitionIn.Begin();
        }

        public void TransitionOut()
        {
            EventHandler<object> onCompleted = null;
            onCompleted = (s, e) =>
            {
                this.sb_TransitionOut.Completed -= onCompleted;
                var handler = this.TransitionOutComplete;
                if (handler != null) handler(this, null);
            };
            this.sb_TransitionOut.Completed += onCompleted;
            this.sb_TransitionOut.Begin();
        }

        public void Init(double width, double height, string image1Asset, string image2Asset, CompositeTransform ct)
        {
            if (width != -1.0)
            {
                base.Width = width;
                base.Height = height;
                this.LayoutRoot.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0.0, 0.0, base.Width, base.Height)
                };
            }

            this._image1Asset = image1Asset;
            var bmp1 = new BitmapImage
            {
                UriSource = new Uri(MainPage._s_Instance.BaseUri, this._image1Asset)
            };
            this._image1.Stretch = Stretch.UniformToFill;
            this._image1.Source = bmp1;
            this._image1.DataContext = this._image1Asset;

            if (ct != null)
                this._image1.RenderTransform = ct;
            else
                this._image1.RenderTransform = new CompositeTransform();

            this._image2Asset = image2Asset;
            var bmp2 = new BitmapImage
            {
                UriSource = new Uri(MainPage._s_Instance.BaseUri, this._image2Asset)
            };
            this._image2.Stretch = Stretch.UniformToFill;
            this._image2.Source = bmp2;
            this._image2.DataContext = this._image2Asset;
        }

        public void BeginEdit()
        {
            this._isEditEnabled = true;
        }

        public void CompleteEdit()
        {
            var ct = this._image1.RenderTransform as CompositeTransform;
            this._isEditEnabled = false;
            this.SceneData = new SceneData
            {
                Image1Asset = this._image1Asset,
                Image2Asset = this._image2Asset,
                CompositeTransform = ct
            };
        }

        private void _panelEditCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (this._isInputCaptured && this._isEditEnabled)
            {
                this._image1.ReleasePointerCapture(e.Pointer);
                this._isInputCaptured = false;
            }
        }

        private void _panelEditCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this._isInputCaptured && this._isEditEnabled)
            {
                var ct = this._image1.RenderTransform as CompositeTransform;
                double dx = e.GetCurrentPoint(null).Position.X - this._pointerPos.Position.X;
                double dy = e.GetCurrentPoint(null).Position.Y - this._pointerPos.Position.Y;
                ct.TranslateX += dx;
                ct.TranslateY += dy;
                this._pointerPos = e.GetCurrentPoint(null);
            }
        }

        private void _panelEditCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (this._isEditEnabled)
            {
                this._pointerPos = e.GetCurrentPoint(null);
                this._isInputCaptured = true;
                this._image1.CapturePointer(e.Pointer);
            }
        }
    }
}