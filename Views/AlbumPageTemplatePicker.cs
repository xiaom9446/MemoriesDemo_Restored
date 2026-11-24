using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace MemoriesDemo.Views
{
    public sealed partial class AlbumPageTemplatePicker : UserControl, INavigationElement
    {
        public event EventHandler TransitionInComplete;
        public event EventHandler TransitionOutComplete;

        public PageVM DialogResult { get; private set; }

        public AlbumPageTemplatePicker()
        {
            this.InitializeComponent();

            this._gridView1.AllowDrop       = false;
            this._gridView1.CanDragItems    = false;
            this._gridView1.CanReorderItems = false;
            this._gridView1.SelectionMode   = ListViewSelectionMode.None;
            this._gridView1.IsItemClickEnabled = true;
            this._gridView1.ItemClick += OnGridView1_ItemClick;

            this._btn_Cancel.Click += OnBtn_Cancel_Click;
        }

        public string GetUriString()
        {
            return "AlbumPageTemplatePicker.xaml";
        }

        public void TransitionIn()
        {
            this.OnTransitionIn();

            this.sb_TransitionIn.Completed += (s, e) =>
            {
                var handler = this.TransitionInComplete;
                if (handler != null) handler(this, null);
            };
            this.sb_TransitionIn.Begin();
        }

        public void TransitionOut()
        {
            this.sb_TransitionOut.Completed += (s, e) =>
            {
                var handler = this.TransitionOutComplete;
                if (handler != null) handler(this, null);
            };
            this.sb_TransitionOut.Begin();
        }

        private void OnTransitionIn()
        {
            foreach (object obj in MainViewModel.Instance.PageTemplates)
            {
                string templateId = obj as string;

                var previewPage = new PageVM();
                previewPage.PageID = templateId;
                previewPage.MediaItems = new List<MediaItemVM>
                {
                    new MediaItemVM { MediaSource = "Assets\\Daughter Growing Up\\AnandH-28 (860x591).jpg", PageInnerIndex = 0 },
                    new MediaItemVM { MediaSource = "Assets\\Daughter Growing Up\\AnandH-28 (860x591).jpg", PageInnerIndex = 1 },
                    new MediaItemVM { MediaSource = "Assets\\Daughter Growing Up\\AnandH-28 (860x591).jpg", PageInnerIndex = 2 },
                    new MediaItemVM { MediaSource = "Assets\\Daughter Growing Up\\AnandH-28 (860x591).jpg", PageInnerIndex = 3 }
                };

                IGenericAlbumPageView pageView = Helper.GetViewFromTemplateID(templateId);

                var pageViewElement = pageView as FrameworkElement;
                pageViewElement.DataContext = previewPage;

                pageView.LayoutRoot.Height = 145.0;
                pageView.LayoutRoot.Width  = 195.0;

                var uc = pageView as UserControl;
                uc.Tag = templateId;

                this._gridView1.Items.Add(pageView);
            }
        }

        private void OnGridView1_ItemClick(object sender, ItemClickEventArgs e)
        {
            string templateId = ((UserControl)e.ClickedItem).Tag as string;

            this.DialogResult = new PageVM
            {
                PageID    = templateId,
                IsEditing = true
            };

            this.TransitionOut();
        }

        private void OnBtn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = null;
            this.TransitionOut();
        }
    }
}