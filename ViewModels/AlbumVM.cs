// AlbumVM.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using MemoriesDemo.Helpers;

namespace MemoriesDemo.ViewModels
{
    public partial class AlbumVM : INotifyPropertyChanged
    {
        private string _caption;
        private Guid _albumID;
        private string _image;
        private List<MediaItemVM> _mediaItems;
        private string _themeName;
        private string _title;
        private DateTime _dateAdded;
        private ObservableList _pages;
        private string _createdBy;
        private string _templateID;
        private PageVM _currentPage;
        private bool _isEditing;
        private ObservableList _allMediaItems;
        private bool _triggerDisplayAnimation;
        private bool _disableDisplayAnimations;

        public AlbumVM()
        {
            this.Title = "New Album";
            this.MediaItems = new List<MediaItemVM>();
            this.DateAdded = DateTime.UtcNow;
            this.CreatedBy = "New user";
            this.TemplateID = "AlbumItemTemplateView";
            this.AlbumID = Guid.NewGuid();
            this.Caption = "";
            this.Image = "";
            this.ThemeName = "";
            this.Pages = new ObservableList(new List<object>());
            // 必须在这里给出空集合：新建相册到首次 RefreshMediaList 之间，
            // AllMediaItems 为 null 会让所有 .Count / .Remove 调用抛空引用。
            this.AllMediaItems = new ObservableList(new List<object>());
        }

        public event EventHandler DisplayAnimationCompleted;
        public event EventHandler DeletingAlbum;
        public event EventHandler<ShowAlbumEventArgs> ShowAlbumMenu;
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid AlbumID
        {
            get { return this._albumID; }
            set { this._albumID = value; this.OnPropertyChanged("AlbumID"); }
        }

        public DateTime DateAdded
        {
            get { return this._dateAdded; }
            set { this._dateAdded = value; this.OnPropertyChanged("DateAdded"); }
        }

        public string Caption
        {
            get { return this._caption; }
            set { this._caption = value; this.OnPropertyChanged("Caption"); }
        }

        public string Title
        {
            get { return this._title; }
            set { this._title = value; this.OnPropertyChanged("Title"); }
        }

        public string Image
        {
            get { return this._image; }
            set { this._image = value; this.OnPropertyChanged("Image"); }
        }

        public List<MediaItemVM> MediaItems
        {
            get { return this._mediaItems; }
            set { this._mediaItems = value; this.OnPropertyChanged("MediaItems"); }
        }

        public string ThemeName
        {
            get { return this._themeName; }
            set { this._themeName = value; this.OnPropertyChanged("ThemeName"); }
        }

        public ObservableList Pages
        {
            get { return this._pages; }
            set { this._pages = value; this.OnPropertyChanged("Pages"); }
        }

        public ObservableList AllMediaItems
        {
            get { return this._allMediaItems; }
            set { this._allMediaItems = value; this.OnPropertyChanged("AllMediaItems"); }
        }

        public PageVM CurrentPage
        {
            get { return this._currentPage; }
            set { this._currentPage = value; this.OnPropertyChanged("CurrentPage"); }
        }

        public bool IsEditing
        {
            get { return this._isEditing; }
            set { this._isEditing = value; this.OnPropertyChanged("IsEditing"); }
        }

        public string CreatedBy
        {
            get { return this._createdBy; }
            set { this._createdBy = value; this.OnPropertyChanged("CreatedBy"); }
        }

        public string TemplateID
        {
            get { return this._templateID; }
            set { this._templateID = value; this.OnPropertyChanged("TemplateID"); }
        }

        public bool TriggerDisplayAnimations
        {
            get { return this._triggerDisplayAnimation; }
            set { this._triggerDisplayAnimation = value; this.OnPropertyChanged("TriggerDisplayAnimations"); }
        }

        public bool DisableDisplayAnimations
        {
            get { return this._disableDisplayAnimations; }
            set { this._disableDisplayAnimations = value; this.OnPropertyChanged("DisableDisplayAnimations"); }
        }

        public bool IsGenerated { get; set; }
        public bool IsNewAlbum { get; set; }
        public bool IsDemoAlbum { get; set; }

        public void OnDisplayAnimationCompleted()
        {
            if (this.DisplayAnimationCompleted != null)
            {
                this.DisplayAnimationCompleted(this, EventArgs.Empty);
            }
        }

        public void OnDeletingAlbum()
        {
            var deleteTask = DataServices.DeleteAlbumFromStorageAsync(this);
            if (this.DeletingAlbum != null)
            {
                this.DeletingAlbum(this, EventArgs.Empty);
            }
        }

        public void OnShowAlbumMenu(IAlbumItemTemplateView albumItemTemplateView)
        {
            if (this.ShowAlbumMenu != null)
            {
                this.ShowAlbumMenu(this, new ShowAlbumEventArgs(albumItemTemplateView));
            }
        }

        public void RefreshMediaList()
        {
            List<object> list = new List<object>();
            foreach (object obj in this.Pages)
            {
                PageVM page = obj as PageVM;
                if (page != null && page.MediaItems != null)
                {
                    foreach (MediaItemVM mediaItemVM in page.MediaItems)
                    {
                        if (!string.IsNullOrEmpty(mediaItemVM.MediaSource))
                        {
                            list.Add(mediaItemVM);
                        }
                    }
                }
            }
            if (this.MediaItems != null)
            {
                foreach (MediaItemVM mediaItemVM2 in this.MediaItems)
                {
                    if (!string.IsNullOrEmpty(mediaItemVM2.MediaSource))
                    {
                        list.Add(mediaItemVM2);
                    }
                }
            }
            this._allMediaItems = new ObservableList(list);
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