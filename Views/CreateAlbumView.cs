using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MemoriesDemo.Views
{
    public sealed partial class CreateAlbumView : Page, INavigationElement
    {
        private AlbumVM _albumInstance;
        private CoreDispatcher _cd;

        public event EventHandler TransitionInComplete;
        public event EventHandler TransitionOutComplete;

        public CreateAlbumView()
        {
            this.InitializeComponent();

            this._cd = Window.Current.Dispatcher;

            this._cbox_PageMenu.ItemsSource = new List<object> { "Bulk Import" };
            this._cbox_PageMenu.SelectionChanged += OnPageMenu_SelectionChanged;

            this._btn_Media.Click    += OnBtn_Media_Click;
            this._btn_Page.Click     += OnBtn_Page_Click;
            this._btn_EditMode.Click += OnBtn_EditMode_Click;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.TransitionIn();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.TransitionOut();
        }

        public string GetUriString()
        {
            return "CreateAlbumView.xaml";
        }

        public void TransitionIn()
        {
            this.OnTransitionIn();

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
            this.OnTransitionOut();

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
            if (MainViewModel.Instance.CurrentAlbum != null)
                this._albumInstance = MainViewModel.Instance.CurrentAlbum;
            else
                this._albumInstance = new AlbumVM();

            this._albumInstance.IsEditing = true;
            base.DataContext = this._albumInstance;
            MainViewModel.Instance.CurrentMediaItemVM = null;
            MainViewModel.Instance.CurrentView = View.CreateAlbumView;

            this.SetupMediaItems();
        }

        private void OnTransitionOut()
        {
            this._gridView1.SelectionChanged -= OnGridView1_SelectionChanged;

            foreach (MediaItemVM item in this._albumInstance.AllMediaItems)
            {
                item.ShowingContextMenu -= OnShowingContextMenu;
                item.DeletingItem       -= OnDeletingItem;
                item.EditingItem        -= OnEditingItem;
            }
        }

        private void SetupMediaItems()
        {
            this._albumInstance.RefreshMediaList();
            this._gridView1.ItemsSource = this._albumInstance.AllMediaItems;

            if (this._albumInstance.AllMediaItems.Count > 0)
            {
                foreach (MediaItemVM item in this._albumInstance.AllMediaItems)
                {
                    item.ShowingContextMenu += OnShowingContextMenu;
                    item.DeletingItem       += OnDeletingItem;
                    item.EditingItem        += OnEditingItem;
                }
                this._gridView1.SelectionChanged += OnGridView1_SelectionChanged;
                this._gridView1.SelectedIndex = 0;
            }
        }

        private void OnGridView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._gridView1.SelectedItem != null)
            {
                var item = this._gridView1.SelectedItem as MediaItemVM;
                this.UpdatePreviewPanel(item);
            }
        }

        private void UpdatePreviewPanel(MediaItemVM mediaItem)
        {
            this._panel_Preview.DataContext = mediaItem;

            if (mediaItem.SceneData != null && !string.IsNullOrEmpty(mediaItem.SceneData.Image2Asset))
            {
                this.SetupScene(mediaItem, mediaItem.SceneData.Image2Asset);
            }
            else
            {
                MediaPageItemViewHelper.UpdateMedia(
                    mediaItem, this._panel_ImagePreview, this._panel_ImagePreview, null);
            }
            MainViewModel.Instance.CurrentMediaItemVM = mediaItem;
        }

        private void SetupProppedScene(MediaItemVM mediaItem)
        {
            this.InitImageToEdit(mediaItem);

            foreach (PropData prop in mediaItem.Props)
            {
                var propUC = new PropUserControl();
                propUC.Init(this._panel_ImagePreview.Width, this._panel_ImagePreview.Height, prop);
                propUC.BeginEdit();
                this._panel_ImagePreview.Children.Add(propUC);
            }
        }

        private void SetupScene(MediaItemVM mediaItem, string sceneID)
        {
            this._panel_ImagePreview.Children.Clear();

            var sceneUC = new SceneUserControl();
            sceneUC.Init(500.0, 400.0, mediaItem.MediaSource, sceneID, null);
            this._panel_ImagePreview.Children.Add(sceneUC);
        }

        private void InitImageToEdit(MediaItemVM mediaItem)
        {
            var bmp = new BitmapImage();
            bmp.UriSource = new Uri(MainPage._s_Instance.BaseUri, mediaItem.MediaSource);
            bmp.DecodePixelHeight = (int)this._panel_ImagePreview.Height;
            bmp.DecodePixelWidth  = (int)this._panel_ImagePreview.Width;

            var image = new Image
            {
                Width = this._panel_ImagePreview.Width,
                Height = this._panel_ImagePreview.Height,
                Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill,
                Source = bmp,
                RenderTransform = new Windows.UI.Xaml.Media.CompositeTransform()
            };
            this._panel_ImagePreview.Children.Clear();
            this._panel_ImagePreview.Children.Add(image);
        }

        private void OnEditingItem(object sender, EventArgs e)
        {
            var item = sender as MediaItemVM;
            MainViewModel.Instance.CurrentMediaItemVM = item;
            MainViewModel.Instance.TempCurrentAlbumCopy = this._albumInstance;

            var editView = new EditMediaView();
            editView.TransitionOutComplete += OnEditMediaDialogComplete;
            DialogHelper.ShowDialog(editView);
        }

        private void OnDeletingItem(object sender, EventArgs e)
        {
            var item = sender as MediaItemVM;
            if (item != null)
            {
                item.ShowingContextMenu -= OnShowingContextMenu;
                item.DeletingItem       -= OnDeletingItem;
                item.EditingItem        -= OnEditingItem;
                this._albumInstance.AllMediaItems.Remove(item);
            }
        }

        private void OnShowingContextMenu(object sender, EventArgs e)
        {
        }

        private void OnEditMediaDialogComplete(object sender, object e)
        {
            (sender as INavigationElement).TransitionOutComplete -= OnEditMediaDialogComplete;
            if (MainViewModel.Instance.CurrentMediaItemVM != null)
                this.UpdatePreviewPanel(MainViewModel.Instance.CurrentMediaItemVM);
        }

        private void OnPageMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._cbox_PageMenu.SelectedIndex == 0)
                this.LaunchMediaPicker();
        }

        private async void LaunchMediaPicker()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");

            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();
            if (files == null || files.Count == 0) return;

            await this._cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (StorageFile file in files)
                {
                    this._albumInstance.MediaItems.Add(new MediaItemVM
                    {
                        MediaSource = file.Path
                    });
                }
                this.SetupMediaItems();
            });
        }

        private void OnBtn_Media_Click(object sender, RoutedEventArgs e)
        {
        }

        private void OnBtn_Page_Click(object sender, RoutedEventArgs e)
        {
            MainPage.StartAlbumDetailsViewTransitionAnimation();
            MainViewModel.Instance.CurrentAlbum = this._albumInstance;
            base.Frame.Navigate(typeof(AlbumDetailsView));
        }

        private void OnBtn_EditMode_Click(object sender, RoutedEventArgs e)
        {
            MainPage.StartAlbumDetailsViewTransitionAnimation();
            this._albumInstance.IsEditing = false;
            MainViewModel.Instance.CurrentAlbum = this._albumInstance;
            base.Frame.Navigate(typeof(AlbumDetailsView));
        }
    }
}