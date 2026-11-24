using System;
using Windows.UI.Xaml;

namespace MemoriesDemo.Helpers
{
    public interface INavigationElement
    {
        string GetUriString();
        void TransitionIn();
        void TransitionOut();

        event EventHandler TransitionInComplete;
        event EventHandler TransitionOutComplete;
    }
}