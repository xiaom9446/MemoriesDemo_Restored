// MediaItemVM.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MemoriesDemo.ViewModels
{
    public partial class MediaItemVM : INotifyPropertyChanged
    {
        private Guid _mediaItemID;
        private string _mediaSource;
        private string _caption;
        private string _caption2;
        private bool _isPlaying;
        private bool _isReadOnly;

        public MediaItemVM(string mediaSource)
        {
            this.MediaSource = mediaSource;
            this.MediaItemID = Guid.NewGuid();
            this.Props = new List<PropData>();
            this.CompositeTransform = new CompositeTransformData();
        }

        public MediaItemVM()
        {
            this.MediaItemID = Guid.NewGuid();
            this.Props = new List<PropData>();
            this.CompositeTransform = new CompositeTransformData();
            this.MediaSource = "";
        }

        public Guid MediaItemID
        {
            get { return this._mediaItemID; }
            set
            {
                this._mediaItemID = value;
                this.OnPropertyChanged("MediaItemID");
            }
        }

        public string MediaSource
        {
            get { return this._mediaSource; }
            set
            {
                this._mediaSource = value;
                this.OnPropertyChanged("MediaSource");
            }
        }

        public string Caption
        {
            get { return this._caption; }
            set
            {
                this._caption = value;
                this.OnPropertyChanged("Caption");
            }
        }

        public string Caption2
        {
            get { return this._caption2; }
            set
            {
                this._caption2 = value;
                this.OnPropertyChanged("Caption2");
            }
        }

        public bool IsPlaying
        {
            get { return this._isPlaying; }
            set
            {
                this._isPlaying = value;
                this.OnPropertyChanged("IsPlaying");
            }
        }

        public bool IsReadOnly
        {
            get { return this._isReadOnly; }
            set
            {
                this._isReadOnly = value;
                this.OnPropertyChanged("IsReadOnly");
            }
        }

        public int PageInnerIndex { get; set; }
        public string SceneID { get; set; }
        public CompositeTransformData CompositeTransform { get; set; }
        public List<PropData> Props { get; set; }
        public SceneData SceneData { get; set; }
        public bool IsMediaFromStream { get; set; }

        public event EventHandler ShowingContextMenu;
        public event EventHandler DeletingItem;
        public event EventHandler EditingItem;
        public event PropertyChangedEventHandler PropertyChanged;

        public void ShowContextMenu()
        {
            if (this.ShowingContextMenu != null)
            {
                this.ShowingContextMenu(this, EventArgs.Empty);
            }
        }

        public void DeleteItem()
        {
            if (this.DeletingItem != null)
            {
                this.DeletingItem(this, EventArgs.Empty);
            }
        }

        public void EditItem()
        {
            if (this.EditingItem != null)
            {
                this.EditingItem(this, EventArgs.Empty);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}