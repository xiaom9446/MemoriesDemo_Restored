using System;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace MemoriesDemo.Views
{
    public sealed partial class MediaItemEditView : UserControl
    {
        private MediaItemVM _mediaItemVM;
        private Canvas _elementOutsidePopup;

        public event EventHandler PopupClosing;
        public event EventHandler<object> DeleteItem;
        public event EventHandler<object> EditItemClick;

        public MediaItemEditView()
        {
            this.InitializeComponent();

            this._btn_Edit.Click   += OnBtn_Edit_Click;
            this._btn_Delete.Click += OnBtn_Delete_Click;
            this._btn_Delete.IsEnabled = false;

            this.Holding     += OnHolding;
            this.RightTapped += OnRightTapped;

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
            this.Init(base.DataContext as MediaItemVM);
        }

        private void Init(MediaItemVM mediaItemVM)
        {
            this._mediaItemVM = mediaItemVM;
            MediaPageItemViewHelper.UpdateMedia(this._mediaItemVM, this._panel_Img, this.LayoutRoot, null);
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            this._mediaItemVM.ShowContextMenu();
            this.ShowPopup();
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            this._mediaItemVM.ShowContextMenu();
            this.ShowPopup();
        }

        public void ShowPopup()
        {
            this._popup.IsOpen = true;

            var transform = base.TransformToVisual(MainPage._s_Instance.LayoutRoot);
            Point point = transform.TransformPoint(default(Point));
            this._elementOutsidePopup.RenderTransform = new CompositeTransform
            {
                TranslateX = -point.X,
                TranslateY = -point.Y
            };

            VisualStateManager.GoToState(this, "Editing", true);
        }

        public void HidePopup()
        {
            this._popup.IsOpen = false;
            var handler = this.PopupClosing;
            if (handler != null) handler(this, null);
            VisualStateManager.GoToState(this, "NotEditing", true);
        }

        private void OnElementOutsidePopup_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.HidePopup();
        }

        private void OnBtn_Edit_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this._mediaItemVM.EditItem();
            var handler = this.EditItemClick;
            if (handler != null) handler(this, e);
        }

        private void OnBtn_Delete_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this._mediaItemVM.DeleteItem();
            var handler = this.DeleteItem;
            if (handler != null) handler(this, e);
        }
    }
}