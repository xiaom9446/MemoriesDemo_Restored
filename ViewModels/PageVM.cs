// PageVM.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using MemoriesDemo.Helpers;
using MemoriesDemo.Views;

namespace MemoriesDemo.ViewModels
{
    public partial class PageVM : INotifyPropertyChanged
    {
        private bool _isEditing;
        private bool _triggerUnload;
        private bool _isReadOnly;

        public PageVM()
        {
            this.MediaItems = new List<MediaItemVM>();
            this.Captions = new List<TextPageItemVM>();
            this.PageVMID = Guid.NewGuid();
            this.PageID = "";
            this._triggerUnload = false;
            this._isReadOnly = false;
            this.Props = new ObservableList(new List<object>());
        }

        public Guid PageVMID { get; set; }
        public string PageID { get; set; }
        public List<MediaItemVM> MediaItems { get; set; }
        public List<TextPageItemVM> Captions { get; set; }
        public ObservableList Props { get; private set; }

        public bool IsEditing
        {
            get { return this._isEditing; }
            set
            {
                this._isEditing = value;
                this.OnPropertyChanged("IsEditing");
            }
        }

        public bool TriggerUnload
        {
            get { return this._triggerUnload; }
            set { this._triggerUnload = value; }
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

        public void OnShowAppBarMediaMenuItems(MediaPageItemView mediaPageItemView)
        {
            if (this.ShowAppBarMediaMenuItems != null)
            {
                this.ShowAppBarMediaMenuItems(this, new MenuItemEventArgs(mediaPageItemView));
            }
        }

        public event EventHandler<MenuItemEventArgs> ShowAppBarMediaMenuItems;
        public event PropertyChangedEventHandler PropertyChanged;

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