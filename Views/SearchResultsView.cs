using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;

namespace MemoriesDemo.Views
{
    public sealed partial class SearchResultsView : Page
    {
        private Random _random;
        private string _queryString;
        private bool _isNavigating;

        public SearchResultsView()
        {
            this.InitializeComponent();

            this._isNavigating = false;

            this._gridView1.ItemClick += OnGridView1_ItemClick;
            this._gridView2.ItemClick += OnGridView1_ItemClick;

            this._gridView1.CanDragItems = false;
            this._gridView1.CanReorderItems = false;
            this._gridView1.ItemTemplateSelector = new AlbumItemTemplateSelector();

            this._gridView2.CanDragItems = false;
            this._gridView2.CanReorderItems = false;

            this.gridPanelHolder.MinWidth = Window.Current.Bounds.Width;
            this._random = new Random((int)DateTime.Now.Ticks);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (MainPage._s_Instance != null)
                MainPage._s_Instance.NavigationInProgress = true;

            this._queryString = e.Parameter as string;
            base.DataContext = MainViewModel.Instance;
            MainViewModel.Instance.CurrentView = View.SearchResultsView;

            var results = new ObservableList(new List<object>());
            if (!string.IsNullOrEmpty(this._queryString))
            {
                foreach (AlbumVM album in MainViewModel.Instance.Albums)
                {
                    if (album.Title.ContainsString(this._queryString))
                        results.Add(album);
                }
                this._gridView1.ItemsSource = results;
                this._gridView2.ItemsSource = results;
            }

            this._tb_TotalAlbums.Text = MainViewModel.Instance.Albums.Count + " Albums";

            EventHandler<object> onCompleted = null;
            onCompleted = (s, e2) =>
            {
                this.sb_TransitionIn.Completed -= onCompleted;
                if (MainPage._s_Instance != null)
                    MainPage._s_Instance.NavigationInProgress = false;
            };
            this.sb_TransitionIn.Completed += onCompleted;
            this.sb_TransitionIn.Begin();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.sb_TransitionOut.Begin();
        }

        private void OnGridView1_ItemClick(object sender, ItemClickEventArgs e)
        {
            AlbumVM album = e.ClickedItem as AlbumVM;
            if (album != null)
            {
                MainViewModel.Instance.CurrentAlbum = album;
                MainViewModel.Instance.IsNewAlbum = false;
                this.TransitionOutGridView(null);
            }
        }

        private void TransitionOutGridView(AlbumVM skipItem)
        {
            var sb = new Storyboard();
            sb.Completed += (s, e) => base.Frame.Navigate(typeof(AlbumDetailsView));

            double delay = 0.0;
            var duration = new Duration(TimeSpan.FromMilliseconds(100.0));
            int count = Math.Min(6, MainViewModel.Instance.Albums.Count);

            for (int i = 0; i < count; i++)
            {
                var item = this._gridView1.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (item == null) continue;

                var album = item.DataContext as AlbumVM;
                if (album == skipItem) continue;

                if (!(item.RenderTransform is CompositeTransform))
                    item.RenderTransform = new CompositeTransform { TranslateX = 0.0, TranslateY = 0.0 };

                var slide = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1000.0,
                    Duration = duration,
                    BeginTime = TimeSpan.FromMilliseconds(delay),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                Storyboard.SetTarget(slide, item);
                Storyboard.SetTargetProperty(slide, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                sb.Children.Add(slide);

                var fade = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = duration,
                    BeginTime = TimeSpan.FromMilliseconds(delay),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(fade, item);
                Storyboard.SetTargetProperty(fade, "(UIElement.Opacity)");
                sb.Children.Add(fade);

                delay += 100.0;
            }

            var headerFade = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = duration,
                BeginTime = TimeSpan.FromMilliseconds(100.0),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(headerFade, this._panel_Header);
            Storyboard.SetTargetProperty(headerFade, "(UIElement.Opacity)");
            sb.Children.Add(headerFade);

            var subFade = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = duration,
                BeginTime = TimeSpan.FromMilliseconds(100.0),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(subFade, this._subPanel1);
            Storyboard.SetTargetProperty(subFade, "(UIElement.Opacity)");
            sb.Children.Add(subFade);

            sb.Begin();
            MainPage.StartAlbumDetailsViewTransitionAnimation();
        }
    }
}