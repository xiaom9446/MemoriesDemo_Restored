using System;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace MemoriesDemo.Views
{
    public sealed partial class PropUserControl : UserControl, INavigationElement
    {
        private string _imageAsset;
        private bool _isInputCaptured;
        private PointerPoint _pointerPos;
        private bool _isEditEnabled;
        private Canvas _elementOutsidePopup;

        public event EventHandler<object> DeleteItem;
        public event EventHandler TransitionInComplete;
        public event EventHandler TransitionOutComplete;

        public PropData PropData { get; private set; }

        public PropUserControl()
        {
            this.InitializeComponent();

            this._isEditEnabled = false;

            this._btn_Delete.Click    += OnBtn_Delete_Click;
            this._btn_ScaleUp.Click   += OnBtn_ScaleUp_Click;
            this._btn_ScaleDown.Click += OnBtn_ScaleDown_Click;

            this._elementOutsidePopup = new Canvas
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Opacity = 0.4,
                Width = 1476.0,
                Height = 900.0
            };
            this._elementOutsidePopup.PointerPressed += OnElementOutsidePopup_PointerPressed;

            var canvas = new Canvas();
            this._popup.Child = canvas;
            canvas.Children.Add(this._elementOutsidePopup);
            canvas.Children.Add(this._popupChild);

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        public string GetUriString()
        {
            return "PropUserControl.xaml";
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

        public void Init(double width, double height, PropData propData)
        {
            base.Width = width;
            base.Height = height;

            double sx = 1.0;
            double sy = 1.0;

            if (propData.CompositeTransform != null &&
                (width != propData.PanelCanvasWidth || height != propData.PanelCanvasHeight) &&
                (Math.Abs(width - propData.PanelCanvasWidth) > 1.0 || Math.Abs(height - propData.PanelCanvasHeight) > 1.0))
            {
                sx = width / propData.PanelCanvasWidth;
                sy = height / propData.PanelCanvasHeight;
            }

            this._imageAsset = propData.ImageAsset;

            var bmp = new BitmapImage
            {
                DecodePixelHeight = (int)(100.0 * sx),
                DecodePixelWidth  = (int)(100.0 * sx),
                UriSource = new Uri(MainPage._s_Instance.BaseUri, this._imageAsset)
            };

            this._image.Width = 100.0 * sx;
            this._image.Height = 100.0 * sx;
            this._image.Stretch = Stretch.UniformToFill;
            this._image.Source = bmp;
            this._image.DataContext = this._imageAsset;

            this.PropData = propData;
            this.PropData.PanelCanvasHeight = base.Height;
            this.PropData.PanelCanvasWidth  = base.Width;

            if (propData.CompositeTransform != null)
            {
                var ct = new CompositeTransform
                {
                    TranslateX = propData.CompositeTransform.TranslateX,
                    TranslateY = propData.CompositeTransform.TranslateY,
                    ScaleX = propData.CompositeTransform.ScaleX,
                    ScaleY = propData.CompositeTransform.ScaleY
                };
                this._image.RenderTransform = ct;
                ct.TranslateX = ct.TranslateX * sx;
                ct.TranslateY = ct.TranslateY * sy;
            }
            else
            {
                this.PropData.CompositeTransform = new CompositeTransformData();
                this._image.RenderTransform = Helper.CompositeTransformFromCTData(this.PropData.CompositeTransform);
            }

            this._image.ManipulationMode = ManipulationModes.All;
            this._image.ManipulationDelta += OnImage_ManipulationDelta;
            this._image.Holding           += OnHolding;
            this._image.RightTapped       += OnRightTapped;
        }

        public void BeginEdit()
        {
            this._isEditEnabled = true;
        }

        public void CompleteEdit()
        {
            var ct = this._image.RenderTransform as CompositeTransform;
            this._isEditEnabled = false;
            this.PropData.CompositeTransform = Helper.CTDataFromCompositeTransform(ct);
        }

        private void OnImage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!this._isEditEnabled) return;

            var ct = this._image.RenderTransform as CompositeTransform;

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

        public void ShowPopup(Point pos)
        {
            if (!this._isEditEnabled) return;

            this._popupRoot.IsHitTestVisible = true;
            this._popup.IsOpen = true;

            var transform = base.TransformToVisual(MainPage._s_Instance.LayoutRoot);
            Point point = transform.TransformPoint(default(Point));
            this._elementOutsidePopup.RenderTransform = new CompositeTransform
            {
                TranslateX = -point.X,
                TranslateY = -point.Y
            };

            this._popupChild.RenderTransform = new CompositeTransform
            {
                TranslateX = pos.X - this._image.Width * 1.5,
                TranslateY = pos.Y - this._image.Height
            };

            VisualStateManager.GoToState(this, "Editing", true);
        }

        public void HidePopup()
        {
            this._popupRoot.IsHitTestVisible = false;
            this._popup.IsOpen = false;
            VisualStateManager.GoToState(this, "NotEditing", true);
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            this.ShowPopup(e.GetPosition(this._popup));
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            this.ShowPopup(e.GetPosition(this._popup));
        }

        private void OnBtn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!this._isEditEnabled) return;
            this.HidePopup();
            var handler = this.DeleteItem;
            if (handler != null) handler(this, e);
        }

        private void OnBtn_ScaleDown_Click(object sender, RoutedEventArgs e)
        {
            this.PropData.CompositeTransform.ScaleX -= 0.2;
            this.PropData.CompositeTransform.ScaleY -= 0.2;
            var ct = this._image.RenderTransform as CompositeTransform;
            ct.ScaleX -= 0.2;
            ct.ScaleY -= 0.2;
        }

        private void OnBtn_ScaleUp_Click(object sender, RoutedEventArgs e)
        {
            this.PropData.CompositeTransform.ScaleX += 0.2;
            this.PropData.CompositeTransform.ScaleY += 0.2;
            var ct = this._image.RenderTransform as CompositeTransform;
            ct.ScaleX += 0.2;
            ct.ScaleY += 0.2;
        }

        private void OnElementOutsidePopup_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.HidePopup();
        }

        private void _panelEditCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (this._isInputCaptured && this._isEditEnabled)
            {
                this._image.ReleasePointerCapture(e.Pointer);
                this._isInputCaptured = false;
            }
        }

        private void _panelEditCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this._isInputCaptured && this._isEditEnabled)
            {
                var ct = this._image.RenderTransform as CompositeTransform;
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
                this._image.CapturePointer(e.Pointer);
            }
        }
    }
}