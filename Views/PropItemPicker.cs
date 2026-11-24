using System;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace MemoriesDemo.Views
{
    public sealed partial class PropItemPicker : UserControl, INavigationElement
    {
        public event EventHandler TransitionInComplete;
        public event EventHandler TransitionOutComplete;

        public PropData DialogResult { get; private set; }

        public PropItemPicker()
        {
            this.InitializeComponent();

            this._gridView1.AllowDrop       = false;
            this._gridView1.CanDragItems    = false;
            this._gridView1.CanReorderItems = false;
            this._gridView1.SelectionMode   = ListViewSelectionMode.None;
            this._gridView1.IsItemClickEnabled = true;

            this._gridView1.ItemClick += OnGridView1_ItemClick;
            this._btn_Cancel.Click    += OnBtn_Cancel_Click;
        }

        public string GetUriString()
        {
            return "PropItemPicker.xaml";
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
            this.SetupItemPicker();
        }

        private void SetupItemPicker()
        {
            this._gridView1.Items.Clear();
            foreach (object obj in MainViewModel.Instance.PropAssets)
            {
                var bmp = new BitmapImage
                {
                    UriSource = new Uri(MainPage._s_Instance.BaseUri, obj as string),
                    DecodePixelHeight = 100,
                    DecodePixelWidth  = 100
                };

                var image = new Image
                {
                    Width = 100.0,
                    Height = 100.0,
                    Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill,
                    Source = bmp,
                    DataContext = obj,
                    Tag = obj
                };
                this._gridView1.Items.Add(image);
            }
        }

        private void OnGridView1_ItemClick(object sender, ItemClickEventArgs e)
        {
            string imageAsset = (e.ClickedItem as FrameworkElement).Tag as string;
            this.DialogResult = new PropData { ImageAsset = imageAsset };
            this.TransitionOut();
        }

        private void OnBtn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = null;
            this.TransitionOut();
        }
    }
}