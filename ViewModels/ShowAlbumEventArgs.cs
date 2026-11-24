// ShowAlbumEventArgs.cs
using System;
using MemoriesDemo.Helpers;

namespace MemoriesDemo.ViewModels
{
    public partial class ShowAlbumEventArgs : EventArgs
    {
        public ShowAlbumEventArgs(IAlbumItemTemplateView albumItemTemplateView)
        {
            this.AlbumItemTemplateView = albumItemTemplateView;
        }

        public IAlbumItemTemplateView AlbumItemTemplateView { get; set; }
    }
}