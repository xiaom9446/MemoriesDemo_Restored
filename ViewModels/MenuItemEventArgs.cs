// MenuItemEventArgs.cs
using System;
using MemoriesDemo.Views;

namespace MemoriesDemo.ViewModels
{
    public partial class MenuItemEventArgs : EventArgs
    {
        public MenuItemEventArgs(MediaPageItemView mediaPageItemView)
        {
            this.MediaPageItemView = mediaPageItemView;
        }

        public MediaPageItemView MediaPageItemView { get; set; }
    }
}