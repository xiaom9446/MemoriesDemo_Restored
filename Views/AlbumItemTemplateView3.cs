using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace MemoriesDemo.Views
{
    public sealed partial class AlbumItemTemplateView3 : UserControl, IAlbumItemTemplateView
    {
        public Storyboard Sb_DisplayAnimation { get; set; }
        public Storyboard Sb_DisplayAnimation2 { get; set; }

        public static readonly DependencyProperty TriggerDisplayAnimationsProperty =
            DependencyProperty.Register(
                "TriggerDisplayAnimations",
                typeof(bool),
                typeof(AlbumItemTemplateView3),
                new PropertyMetadata(false, OnTriggerDisplayAnimationsPropertyChanged));

        public bool TriggerDisplayAnimations
        {
            get { return (bool)GetValue(TriggerDisplayAnimationsProperty); }
            set { SetValue(TriggerDisplayAnimationsProperty, value); }
        }

        private static void OnTriggerDisplayAnimationsPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as AlbumItemTemplateView3).OnTriggerDisplayAnimationsPropertyChanged(e);
        }

        private void OnTriggerDisplayAnimationsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.RunDisplayAnimation();
        }

        // ===== 供 Helper 使用的外部接口属性 =====
        public Panel Panel_Tile0 { get { return this._panel_Tile0; } set { this._panel_Tile0 = value as Grid; } }
        public Panel Panel_Tile1 { get { return this._panel_Tile1; } set { this._panel_Tile1 = value as Grid; } }
        public Panel Panel_Tile2 { get { return this._panel_Tile2; } set { this._panel_Tile2 = value as Grid; } }
        public Panel Panel_Tile3 { get { return this._panel_Tile3; } set { this._panel_Tile3 = value as Grid; } }
        public Panel Panel_Tile4 { get { return this._panel_Tile4; } set { this._panel_Tile4 = value as Grid; } }

        // View3 特有：Tile5 / Tile6 纯自动属性（无 backing field）
        public Panel Panel_Tile5 { get; set; }
        public Panel Panel_Tile6 { get; set; }

        public Panel Panel_Large  { get { return this._panel_Large;  } set { this._panel_Large  = value as Grid; } }
        public Style _InnerTextBlockStyle_1 { get { return this.InnerTextBlockStyle_1; } set { this.InnerTextBlockStyle_1 = value; } }
        public Panel Panel_Pattern { get { return this._panel_Pattern; } set { this._panel_Pattern = value as Grid; } }
        public Button Btn_Delete { get { return this._btn_Delete; } set { this._btn_Delete = value; } }
        public Popup PopUp { get { return this._popup; } set { this._popup = value; } }
        public FrameworkElement PopupChild { get { return this._popupChild; } set { this._popupChild = value as Grid; } }

        // ===== Helper 内部使用的自动属性 =====
      
        public AlbumVM _albumVM { get; set; }
        public ObservableList _mediaItems { get; set; }
        public bool _isLargeImageDisplayed { get; set; }
        public Random _random { get; set; }
        public List<Panel> PanelTiles { get; set; }
        public Canvas _elementOutsidePopup { get; set; }
        public int _nextAnimationIndex { get; set; }

        public AlbumItemTemplateView3()
        {
            this.InitializeComponent();
            AlbumItemTemplateViewHelper.InitComponent(this);
            if (this.Sb_DisplayAnimation == null) this.Sb_DisplayAnimation = new Storyboard();
            if (this.Sb_DisplayAnimation2 == null) this.Sb_DisplayAnimation2 = new Storyboard();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AlbumItemTemplateViewHelper.Init(this, base.DataContext as AlbumVM);
        }

        public void ShowPopup()
        {
            AlbumItemTemplateViewHelper.ShowPopup(this);
        }

        public void HidePopup()
        {
            AlbumItemTemplateViewHelper.HidePopup(this);
        }

        private void RunDisplayAnimation()
        {
            if (this._random.Next(0, 5) == 0)
            {
                AlbumItemTemplateViewHelper.CreateLargeDisplayAnimation(this);
                return;
            }
            AlbumItemTemplateViewHelper.CreateDisplayAnimations(this);
        }

        private void StopDisplayAnimation()
        {
            
        }

        public void OnElementOutsidePopup_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.HidePopup();
        }

        public void OnBtn_Delete_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopup();
            this._albumVM.OnDeletingAlbum();
            MainViewModel.Instance.Albums.Remove(this._albumVM);
        }

        public void _sb_DisplayAnimation_Completed(object sender, object e)
        {
            AlbumItemTemplateViewHelper.DisplayAnimation_CompletedHelper(this, sender, e);
        }

        public void _sb_DisplayAnimationLarge_Completed(object sender, object e)
        {
            AlbumItemTemplateViewHelper.DisplayAnimationLarge_CompletedHelper(this, sender, e);
        }

        public void OnAlbumItemTemplateView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            this.ShowPopup();
        }

        public void OnAlbumItemTemplateView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            this.ShowPopup();
        }

        public void ClearDPs()
        {
            base.ClearValue(AlbumItemTemplateView3.TriggerDisplayAnimationsProperty);
        }
    }
}