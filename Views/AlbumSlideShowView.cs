using System;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace MemoriesDemo.Views
{
    public sealed partial class AlbumSlideShowView : UserControl
    {
        // ===== 依赖属性 =====
        public static readonly DependencyProperty TriggerDisplayAnimationsProperty =
            DependencyProperty.Register(
                "TriggerDisplayAnimations",
                typeof(bool),
                typeof(AlbumSlideShowView),
                new PropertyMetadata(false, OnTriggerDisplayAnimationsPropertyChanged));

        public bool TriggerDisplayAnimations
        {
            get { return (bool)GetValue(TriggerDisplayAnimationsProperty); }
            set { SetValue(TriggerDisplayAnimationsProperty, value); }
        }

        private static void OnTriggerDisplayAnimationsPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as AlbumSlideShowView).OnTriggerDisplayAnimationsPropertyChanged(e);
        }

        private void OnTriggerDisplayAnimationsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.RunDisplayAnimation();
        }

        public static readonly DependencyProperty DisableDisplayAnimationsProperty =
            DependencyProperty.Register(
                "DisableDisplayAnimations",
                typeof(bool),
                typeof(AlbumSlideShowView),
                new PropertyMetadata(false, OnDisableDisplayAnimationsPropertyChanged));

        public bool DisableDisplayAnimations
        {
            get { return (bool)GetValue(DisableDisplayAnimationsProperty); }
            set { SetValue(DisableDisplayAnimationsProperty, value); }
        }

        private static void OnDisableDisplayAnimationsPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as AlbumSlideShowView).OnDisableDisplayAnimationsPropertyChanged(e);
        }

        private void OnDisableDisplayAnimationsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.StopDisplayAnimation();
        }

        // ===== 业务字段 =====
        private AlbumVM _albumVM;
        private Random _random;
        private Storyboard _sb_SlideShow;
        private Size _desiredSize;

        // ===== 构造函数 =====
        public AlbumSlideShowView()
        {
            this.InitializeComponent();

            this._random = new Random((int)DateTime.Now.Ticks);
            this._desiredSize = new Size(300.0, 280.0);

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this._albumVM = base.DataContext as AlbumVM;
            this._albumVM.RefreshMediaList();

            if (this._albumVM.AllMediaItems.Count > 0)
            {
                MediaItemVM item = null;
                while (item == null || string.IsNullOrEmpty(item.MediaSource))
                {
                    int idx = this._random.Next(0, this._albumVM.AllMediaItems.Count);
                    item = this._albumVM.AllMediaItems[idx] as MediaItemVM;
                }
                MediaPageItemViewHelper.UpdateMedia(
                    item,
                    this._panel_Media,
                    true,
                    this._desiredSize,
                    null);
            }
        }

        private void RunDisplayAnimation()
        {
            this._sb_SlideShow = new Storyboard();
            this._sb_SlideShow.Completed += OnSb_SlideShow_Completed;

            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(100.0)),
                BeginTime = TimeSpan.FromMilliseconds(1000.0),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeOut, this._panel_Media);
            Storyboard.SetTargetProperty(fadeOut, "(UIElement.Opacity)");
            this._sb_SlideShow.Children.Add(fadeOut);

            this._sb_SlideShow.Begin();
        }

        private void OnSb_SlideShow_Completed(object sender, object e)
        {
            MediaItemVM item = null;
            while (item == null || string.IsNullOrEmpty(item.MediaSource))
            {
                int idx = this._random.Next(0, this._albumVM.AllMediaItems.Count);
                item = this._albumVM.AllMediaItems[idx] as MediaItemVM;
            }
            MediaPageItemViewHelper.UpdateMedia(
                item,
                this._panel_Media,
                true,
                this._desiredSize,
                null);

            var sb = new Storyboard();
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(3000.0)),
                BeginTime = TimeSpan.FromMilliseconds(1000.0),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fadeIn, this._panel_Media);
            Storyboard.SetTargetProperty(fadeIn, "(UIElement.Opacity)");
            sb.Children.Add(fadeIn);

            EventHandler<object> onCompleted = null;
            onCompleted = (s, args) =>
            {
                sb.Completed -= onCompleted;
                this._albumVM.OnDisplayAnimationCompleted();
            };
            sb.Completed += onCompleted;
            sb.Begin();
        }

        public void StopDisplayAnimation()
        {
            if (this._sb_SlideShow != null)
            {
                this._sb_SlideShow.Stop();
            }
        }
    }
}