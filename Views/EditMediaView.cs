using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace MemoriesDemo.Views
{
    public sealed partial class EditMediaView : UserControl, INavigationElement
    {
        private MediaItemVM _mediaItemToEdit;
        private List<object> _tempItemsToImport;
        private List<PropUserControl> _props;
        private AdornerType _adornerType;

        public event EventHandler TransitionInComplete;
        public event EventHandler TransitionOutComplete;

        public EditMediaView()
        {
            this.InitializeComponent();

            this._adornerType = AdornerType.Props;
            this._props = new List<PropUserControl>();

            this._btn_Save.Click   += OnBtn_Save_Click;
            this._btn_Cancel.Click += OnBtn_Cancel_Click;
            this._btn_Undo.Click   += OnBtn_Undo_Click;

            this._panelEditCanvas.Drop += OnPanelEditCanvas_Drop;
            this._gridView1.DragItemsStarting += OnGridView1_DragItemsStarting;
        }

        public string GetUriString()
        {
            return "EditMediaView.xaml";
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
            this._props.Clear();
            this._mediaItemToEdit = MainViewModel.Instance.CurrentMediaItemVM;
            base.DataContext = this._mediaItemToEdit;
            MainViewModel.Instance.CurrentView = View.EditMediaView;

            if (this._adornerType == AdornerType.Props)
                this._btn_Props.IsChecked = true;
            else
                this._btn_Scenes.IsChecked = true;

            EventHandler<object> onCompleted = null;
            onCompleted = (s, e) =>
            {
                this.sb_TransitionIn.Completed -= onCompleted;
                this.InitSceneLayout();
                var handler = this.TransitionInComplete;
                if (handler != null) handler(this, null);
            };
            this.sb_TransitionIn.Completed += onCompleted;
            this.sb_TransitionIn.Begin();
        }

        private void OnSb_TransitionGridViewOut_Completed(object sender, object e)
        {
            this._sb_TransitionGridViewOut.Completed -= OnSb_TransitionGridViewOut_Completed;
            this.InitItemPicker();
        }

        private void OnPanelEditCanvas_Drop(object sender, DragEventArgs e)
        {
            if (this._tempItemsToImport == null || this._tempItemsToImport.Count == 0)
                return;

            if (this._adornerType == AdornerType.Props)
            {
                Point position = e.GetPosition(this._panelEditCanvas);

                var ct = new CompositeTransform
                {
                    TranslateX = position.X - this._panelEditCanvas.Width  / 2.0,
                    TranslateY = position.Y - this._panelEditCanvas.Height / 2.0
                };

                string imageAsset = (this._tempItemsToImport[0] as FrameworkElement).DataContext as string;

                var propUC = new PropUserControl();
                propUC.Init(this._panelEditCanvas.Width, this._panelEditCanvas.Height, new PropData
                {
                    ImageAsset             = imageAsset,
                    CompositeTransform     = Helper.CTDataFromCompositeTransform(ct),
                    PanelCanvasHeight      = this._panelEditCanvas.Height,
                    PanelCanvasWidth       = this._panelEditCanvas.Width
                });
                propUC.DeleteItem += OnProp_DeleteItem;
                this._panelEditCanvas.Children.Add(propUC);
                propUC.BeginEdit();

                propUC.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0.0, 0.0, this._panelEditCanvas.Width, this._panelEditCanvas.Height)
                };

                this._props.Add(propUC);
            }

            this._tempItemsToImport.Clear();
            this._tempItemsToImport = null;
        }

        private void OnGridView1_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items == null) return;

            this._tempItemsToImport = new List<object>();
            foreach (object item in e.Items)
            {
                if (item is Image)
                    this._tempItemsToImport.Add(item);
            }
        }

        private void OnProp_DeleteItem(object sender, object e)
        {
            var propUC = sender as PropUserControl;
            propUC.DeleteItem -= OnProp_DeleteItem;
            this._panelEditCanvas.Children.Remove(propUC);
            this._props.Remove(propUC);
        }

        private void OnBtn_Undo_Click(object sender, RoutedEventArgs e)
        {
            this._props.Clear();
            this._mediaItemToEdit.Props.Clear();
            this._mediaItemToEdit.SceneData = null;
            this.InitSceneLayout();
        }

        private void InitSceneLayout()
        {
            this.InitImageToEdit();
            this.InitItemPicker();
        }

        private void InitImageToEdit()
        {
            this._panelEditCanvas.Width = 700.0;
            this._panelEditCanvas.Height = 700.0;

            MediaPageItemViewHelper.UpdateMedia(
                this._mediaItemToEdit,
                this._panelEditCanvas,
                this._panelEditCanvas,
                delegate(object props)
                {
                    var list = props as List<UIElement>;
                    if (list != null)
                    {
                        foreach (UIElement element in list)
                        {
                            var propUC = element as PropUserControl;
                            if (propUC != null)
                            {
                                propUC.BeginEdit();
                                this._props.Add(propUC);
                            }
                        }
                    }
                });
        }

        private void InitItemPicker()
        {
            if (this._adornerType == AdornerType.Props)
                this.SetupItemPicker();

            this._sb_TransitionGridViewIn.Begin();
        }

        private void SetupItemPicker()
        {
            this._gridView1.Items.Clear();
            foreach (object obj in MainViewModel.Instance.PropAssets)
            {
                var bmp = new BitmapImage
                {
                    UriSource = new Uri(MainPage._s_Instance.BaseUri, obj as string),
                    DecodePixelHeight = 80,
                    DecodePixelWidth  = 80
                };

                var image = new Image
                {
                    Width = 80.0,
                    Height = 80.0,
                    Stretch = Stretch.UniformToFill,
                    Source = bmp,
                    DataContext = obj
                };
                this._gridView1.Items.Add(image);
            }
        }

        private void OnBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (this._props.Count > 0)
            {
                this._mediaItemToEdit.Props.Clear();
                foreach (PropUserControl propUC in this._props)
                {
                    propUC.CompleteEdit();
                    this._mediaItemToEdit.Props.Add(propUC.PropData);
                }
            }
            this.TransitionOut();
        }

        private void OnBtn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.CurrentMediaItemVM = null;
            this.TransitionOut();
        }

        private enum AdornerType
        {
            Props,
            Scenes
        }
    }
}