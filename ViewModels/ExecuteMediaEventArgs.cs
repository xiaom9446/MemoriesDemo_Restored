// ExecuteMediaEventArgs.cs
using System;

namespace MemoriesDemo.ViewModels
{
    public partial class ExecuteMediaEventArgs : EventArgs
    {
        public ExecuteMediaEventArgs(object visualElement)
        {
            this.VisualElement = visualElement;
        }

        public object VisualElement { get; set; }
    }
}