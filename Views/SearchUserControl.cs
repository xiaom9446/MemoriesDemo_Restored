using System;
using System.Collections.Generic;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace MemoriesDemo.Views
{
    public partial class SearchUserControl : UserControl
    {
        private Canvas _elementOutsidePopup;

        public event EventHandler PopupClosing;
        public event ItemClickEventHandler ItemClicked;

        public SearchUserControl()
        {
            this.InitializeComponent();

            this._searchButton.Click      += _searchButton_Click;
            this.eraseButton.Click        += eraseButton_Click;
            this.searchBox.KeyDown        += OnSearchBox_KeyDown;
            this._searchButton2.Click     += OnShowResults_Click;
            this._searchListView.ItemClick += OnSearchListView_ItemClick;

            this._elementOutsidePopup = new Canvas
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Width = 2176.0,
                Height = 850.0
            };
            this._elementOutsidePopup.PointerPressed += OnElementOutsidePopup_PointerPressed;

            var canvas = new Canvas();
            this._popup.Child = canvas;
            canvas.Children.Add(this._elementOutsidePopup);
            canvas.Children.Add(this._popupChild);

            this._popupChild.PointerPressed += OnElementOutsidePopup_PointerPressed;
        }

        private void OnSearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                this.OnShowResults_Click(this, null);
        }

        private void OnShowResults_Click(object sender, RoutedEventArgs e)
        {
            var results = new List<object>();
            foreach (AlbumVM album in MainViewModel.Instance.Albums)
            {
                if (album.Title.ToLower().Contains(this.searchBox.Text.ToLower()))
                    results.Add(album);
            }
            this._searchListView.ItemsSource = results;
            VisualStateManager.GoToState(this, "ShowResults", true);
        }

        private void OnSearchListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.HidePopup();
            var handler = this.ItemClicked;
            if (handler != null) handler(this, e);
        }

        private void eraseButton_Click(object sender, RoutedEventArgs e)
        {
            this.searchBox.Text = string.Empty;
            this.searchBox.Focus(FocusState.Programmatic);
        }

        private void _searchButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPopup();
        }

        public void ShowPopup()
        {
            this.searchBox.Text = "Search for...";
            this._searchListView.ItemsSource = null;
            this._popup.IsOpen = true;
            this.searchBox.SelectAll();
            this.searchBox.Focus(FocusState.Programmatic);

            var transform = base.TransformToVisual(MainPage._s_Instance.LayoutRoot);
            Point point = transform.TransformPoint(default(Point));
            this._elementOutsidePopup.RenderTransform = new CompositeTransform
            {
                TranslateX = -point.X,
                TranslateY = -point.Y
            };

            VisualStateManager.GoToState(this, "ShowSearchBox", true);
        }

        public void HidePopup()
        {
            EventHandler<object> onCompleted = null;
            onCompleted = (s, e) =>
            {
                this._sb_TransitionSearchOut.Completed -= onCompleted;
                this._popup.IsOpen = false;
                var handler = this.PopupClosing;
                if (handler != null) handler(this, null);
            };
            this._sb_TransitionSearchOut.Completed += onCompleted;
            this._sb_TransitionSearchOut.Begin();
            VisualStateManager.GoToState(this, "NoSearchBox", true);
        }

        private void OnElementOutsidePopup_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.HidePopup();
        }
    }
}