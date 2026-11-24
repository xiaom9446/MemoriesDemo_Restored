using System;
using MemoriesDemo.Views;
using Windows.UI.Xaml;

namespace MemoriesDemo.Helpers
{
    public partial class DialogHelper
    {
        public static void ShowDialog(INavigationElement dialogView)
        {
            if (dialogView != null)
            {
                dialogView.TransitionOutComplete += OnDialogView_TransitionOutComplete;
                MainPage._s_Instance.LayoutRoot.Children[0].IsHitTestVisible = false;
                MainPage._s_Instance.LayoutRoot.Children.Add(dialogView as UIElement);
                dialogView.TransitionIn();
            }
        }

        private static void OnDialogView_TransitionOutComplete(object sender, object e)
        {
            var navElement = sender as INavigationElement;
            if (navElement != null)
            {
                navElement.TransitionOutComplete -= OnDialogView_TransitionOutComplete;
                MainPage._s_Instance.LayoutRoot.Children.Remove(sender as UIElement);
                MainPage._s_Instance.LayoutRoot.Children[0].IsHitTestVisible = true;
            }
        }
    }
}