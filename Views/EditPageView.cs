using System;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace MemoriesDemo.Views
{
    public sealed partial class EditPageView : Page, INavigationElement
    {
        private PageVM _currentPage;
        private UserControl _pageView;

        public event EventHandler TransitionInComplete;
        public event EventHandler TransitionOutComplete;

        public EditPageView()
        {
            this.InitializeComponent();

            this._btn_Save.Click   += OnBtn_Save_Click;
            this._btn_Cancel.Click += OnBtn_Cancel_Click;
            this._btn_Undo.Click   += OnBtn_Undo_Click;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.OnTransitionIn();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.OnTransitionOut();
        }

        public string GetUriString()
        {
            return "EditPageView.xaml";
        }

        public void TransitionIn()
        {
            this.OnTransitionIn();
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

        private void OnTransitionIn()
        {
            this._currentPage = MainViewModel.Instance.CurrentPage;
            base.DataContext = this._currentPage;
            MainViewModel.Instance.CurrentView = View.EditPageView;

            if (this._currentPage == null) return;

            this.InitLayout();

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

        private void OnTransitionOut()
        {
        }

        private void InitLayout()
        {
            this._pageView = Helper.GetViewFromTemplateID(MainViewModel.Instance.CurrentPage.PageID) as UserControl;
            this._pageView.DataContext = this._currentPage;
            AlbumPageViewHelper.BeginEdit(this._pageView as IGenericAlbumPageView);
            this._panelEditCanvas.Children.Clear();
            this._panelEditCanvas.Children.Add(this._pageView);
        }

        private void OnBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (this._pageView != null)
            {
                AlbumPageViewHelper.CompleteEdit(this._pageView as IGenericAlbumPageView);
            }
            this.TransitionOut();
        }

        private void OnBtn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.CurrentPage = null;
            this.TransitionOut();
        }

        private void OnBtn_Undo_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentPage != null)
            {
                foreach (MediaItemVM item in this._currentPage.MediaItems)
                {
                    item.CompositeTransform = new CompositeTransformData();
                }
            }
            this.InitLayout();
        }
    }
}