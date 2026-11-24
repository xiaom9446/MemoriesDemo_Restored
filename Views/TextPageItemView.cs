using System;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI; 
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace MemoriesDemo.Views
{
    public partial class TextPageItemView : UserControl
    {
        private TextPageItemVM _textPageItemVM;
        private Canvas _elementOutsidePopup;
        private bool _isEditMode;

        public static DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(TextPageItemView),
                new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));

        public TextPageItemView()
        {
            this.InitializeComponent();

            this._elementOutsidePopup = new Canvas();
            this._elementOutsidePopup.Background = new SolidColorBrush(Colors.Transparent);
            this._elementOutsidePopup.Opacity = 0.8;
            this._elementOutsidePopup.Width = 1776.0;
            this._elementOutsidePopup.Height = 1100.0;
            this._elementOutsidePopup.PointerPressed += this.OnElementOutsidePopup_PointerPressed;

            Canvas canvas = new Canvas();
            this._popup.Child = canvas;
            canvas.Children.Add(this._elementOutsidePopup);
            canvas.Children.Add(this._popupChild);

            if (this._tb_TextEdit != null)
            {
                this._tb_TextEdit.TextChanged += this.OnTb_TextEdit_TextChanged;
                this._tb_TextEdit.KeyDown     += this.OnTb_TextEdit_KeyDown;
            }

            this.Loaded += this.OnLoaded;
        }

        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextPageItemView).OnIsReadOnlyPropertyChanged(e);
        }

        private void OnIsReadOnlyPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            bool flag = (bool)e.NewValue;
            if (flag)
            {
                this._tb_AddTextWatermark.Visibility = Visibility.Collapsed;
            }
            else
            {
                this._tb_AddTextWatermark.Visibility = Visibility.Visible;
            }
        }

        public TextBlock TextContent
        {
            get { return this._tb_Text; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.RightTapped += this.OnMediaPageItemView_RightTapped;
            this.Tapped      += this.OnMediaPageItemView_Tapped;
        }

        public void SetupView()
        {
            this._textPageItemVM = this.DataContext as TextPageItemVM;
            this._panel_AddTextWatermark.Visibility = Visibility.Collapsed;
            if (MainViewModel.Instance.CurrentAlbum.IsEditing)
            {
                this.BeginEdit();
                if (string.IsNullOrEmpty(this._textPageItemVM.Caption))
                {
                    this._panel_AddTextWatermark.Visibility = Visibility.Visible;
                }
            }
        }

        public void BeginEdit()
        {
            this._isEditMode = true;
            if (this._textPageItemVM == null)
            {
                this._panel_AddTextWatermark.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(this._textPageItemVM.Caption))
            {
                this._panel_AddTextWatermark.Visibility = Visibility.Visible;
            }
            else
            {
                this._panel_AddTextWatermark.Visibility = Visibility.Collapsed;
            }
        }

        public void CompleteEdit()
        {
            this._isEditMode = false;
            this._panel_AddTextWatermark.Visibility = Visibility.Collapsed;
        }

        private void OnMediaPageItemView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!this.IsReadOnly && this._isEditMode)
            {
                this.ShowPopup(e.GetPosition(this), true);
            }
        }

        private void OnMediaPageItemView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!this.IsReadOnly && this._isEditMode)
            {
                this.ShowPopup(e.GetPosition(this), false);
            }
        }

        private void ShowPopup(Point pos, bool isTapTrigger)
        {
            if (!this._isEditMode || this.IsReadOnly)
            {
                return;
            }

            if (this._textPageItemVM == null)
            {
                return;
            }

            this._popup.IsOpen = true;

            GeneralTransform generalTransform = this.TransformToVisual(MainPage._s_Instance.LayoutRoot);
            Point point = generalTransform.TransformPoint(default(Point));

            CompositeTransform outsideTransform = new CompositeTransform();
            outsideTransform.TranslateX = -point.X;
            outsideTransform.TranslateY = -point.Y;
            this._elementOutsidePopup.RenderTransform = outsideTransform;

            CompositeTransform popupTransform = new CompositeTransform();
            popupTransform.TranslateX = pos.X - this._panel_Text.ActualWidth * 0.85;
            popupTransform.TranslateY = pos.Y - this._panel_Text.ActualHeight * 0.85;
            this._popupChild.RenderTransform = popupTransform;

            generalTransform = this._popupChild.TransformToVisual(MainPage._s_Instance.LayoutRoot);
            point = generalTransform.TransformPoint(default(Point));

            if (point.X < 0.0)
            {
                popupTransform.TranslateX = popupTransform.TranslateX + -point.X;
            }
            else if (point.X + this._popupChild.ActualWidth > Window.Current.Bounds.Width)
            {
                popupTransform.TranslateX = popupTransform.TranslateX - point.X;
            }

            if (point.Y < 0.0)
            {
                popupTransform.TranslateY = popupTransform.TranslateY + -point.Y;
            }

            if (!string.IsNullOrEmpty(this._textPageItemVM.Caption))
            {
                this._tb_TextEdit.Text = this._textPageItemVM.Caption;
            }

            VisualStateManager.GoToState(this, "Editing", true);

            // 自动聚焦，否则用户得先点一下输入框才能打字
            this._tb_TextEdit.Focus(FocusState.Programmatic);
            this._tb_TextEdit.SelectAll();
        }

        private void HidePopup()
        {
            this.SaveCaption();
            this._popup.IsOpen = false;
            VisualStateManager.GoToState(this, "NotEditing", true);
        }

        private void OnElementOutsidePopup_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.HidePopup();
        }

        // 边打边生效：不依赖“点窗外”那一下才保存
        private void OnTb_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._textPageItemVM == null) return;

            this._textPageItemVM.Caption = this._tb_TextEdit.Text;
            this._panel_AddTextWatermark.Visibility = string.IsNullOrEmpty(this._tb_TextEdit.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnTb_TextEdit_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Escape)
            {
                this.HidePopup();
                e.Handled = true;
            }
        }

        private void SaveCaption()
        {
            if (this._textPageItemVM == null) return;

            this._textPageItemVM.Caption = this._tb_TextEdit.Text;
            if (!string.IsNullOrEmpty(this._textPageItemVM.Caption))
            {
                this._panel_AddTextWatermark.Visibility = Visibility.Collapsed;
            }
            else
            {
                this._panel_AddTextWatermark.Visibility = Visibility.Visible;
            }
        }
    }
}