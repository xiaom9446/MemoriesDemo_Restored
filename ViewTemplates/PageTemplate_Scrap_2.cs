using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using MemoriesDemo.Views;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MemoriesDemo.ViewTemplates
{
    public sealed partial class PageTemplate_Scrap_2 : UserControl, IGenericAlbumPageView
    {
        public Panel Panel_Img1 { get { return this._1_panel_Img; } set { this._1_panel_Img = value as Grid; } }
        public Panel Panel_Img2 { get; set; }
        public Panel Panel_Img3 { get; set; }
        public Panel Panel_Img4 { get; set; }
        public Panel Panel_Img5 { get; set; }
        public Panel Panel_Img6 { get; set; }
        public Panel Panel_Img7 { get; set; }
        public Panel Panel_Img8 { get; set; }

        public Panel Tb_Desc1 { get { return this._1_tb_Desc; } }
        public Panel Tb_Desc2 { get { return this._2_tb_Desc; } }
        public Panel Tb_Desc3 { get; set; }
        public Panel Tb_Desc4 { get; set; }
        public Panel Tb_Desc5 { get; set; }
        public Panel Tb_Desc6 { get; set; }
        public Panel Tb_Desc7 { get; set; }
        public Panel Tb_Desc8 { get; set; }

        public Panel LayoutRoot { get { return this._layoutRoot; } }
        public int MaxItems { get; private set; }
        public int MaxTextItems { get; private set; }
        public List<MediaPageItemView> MediaPageItems { get { return this._mediaPageItems; } }
        public PageVM PageVM { get { return this._pageVM; } }
        public Panel Canvas_LargeView { get { return this._canvas_LargeView; } }
        public AlbumPageTransitionInfo TransitionInfo { get; set; }
        public List<PropUserControl> PropViews { get; set; }
        public List<TextPageItemView> TextPageItems { get; set; }
        public Size DesiredViewSize { get; set; }
        public Panel PropsPanel { get { return this._propsView; } }

        // ===== 依赖属性 =====
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(PageTemplate_Scrap_2), new PropertyMetadata(false, OnIsEditingPropertyChanged));
        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } set { SetValue(IsEditingProperty, value); } }
        private static void OnIsEditingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { (d as PageTemplate_Scrap_2).OnIsEditingPropertyChanged(e); }
        private void OnIsEditingPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) AlbumPageViewHelper.BeginEdit(this);
            else AlbumPageViewHelper.CompleteEdit(this);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PageTemplate_Scrap_2), new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));
        public bool IsReadOnly { get { return (bool)GetValue(IsReadOnlyProperty); } set { SetValue(IsReadOnlyProperty, value); } }
        private static void OnIsReadOnlyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { (d as PageTemplate_Scrap_2).OnIsReadOnlyPropertyChanged(e); }
        private void OnIsReadOnlyPropertyChanged(DependencyPropertyChangedEventArgs e) { AlbumPageViewHelper.SetIsReadOnly(this, (bool)e.NewValue); }

        public static readonly DependencyProperty TriggerUnloadProperty =
            DependencyProperty.Register("TriggerUnload", typeof(bool), typeof(PageTemplate_Scrap_2), new PropertyMetadata(false, OnTriggerUnloadPropertyChanged));
        public bool TriggerUnload { get { return (bool)GetValue(TriggerUnloadProperty); } set { SetValue(TriggerUnloadProperty, value); } }
        private static void OnTriggerUnloadPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { (d as PageTemplate_Scrap_2).OnTriggerUnloadPropertyChanged(e); }
        private void OnTriggerUnloadPropertyChanged(DependencyPropertyChangedEventArgs e) { this.OnUnloaded(); }

        // ===== 构造 =====
        public PageTemplate_Scrap_2()
        {
            this.InitializeComponent();
            this._mediaPageItems = new List<MediaPageItemView>();
            this.PropViews = new List<PropUserControl>();
            this.TextPageItems = new List<TextPageItemView>();
            this.MaxItems = 1;
            this.MaxTextItems = 2;
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) { this.DesiredViewSize = e.NewSize; }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this._pageVM = base.DataContext as PageVM;
            AlbumPageViewHelper.Init(this, this._pageVM);
        }

        // ===== 转发 =====
        public void OnExecuteMediaItem(object sender, ExecuteMediaEventArgs args) { AlbumPageViewHelper.OnExecuteMediaItem(this, sender, args); }
        public void OnUnloaded() { AlbumPageViewHelper.OnUnloaded(this); }
        public void OnAnimatedLargeViewIn_Completed(object sender, object e) { AlbumPageViewHelper.OnAnimatedLargeViewIn_Completed(this, sender, e); }
        public void OnAnimatedLargeViewOut_Completed(object sender, object e) { AlbumPageViewHelper.OnAnimatedLargeViewOut_Completed(this, sender, e); }
        public void OnPropUserControl_DeleteItem(object sender, object e) { AlbumPageViewHelper.OnPropUserControl_DeleteItem(this, sender, e); }
        public void ShowAppBarMediaMenuItems(MediaPageItemView mediaPageItemView) { AlbumPageViewHelper.ShowAppBarMediaMenuItems(this, mediaPageItemView); }

        public void ClearDPs()
        {
            base.ClearValue(IsEditingProperty);
            base.ClearValue(IsReadOnlyProperty);
            base.ClearValue(TriggerUnloadProperty);
        }

        // ===== XAML 桩 =====
       private PageVM _pageVM;
private List<MediaPageItemView> _mediaPageItems;

       
    }
}