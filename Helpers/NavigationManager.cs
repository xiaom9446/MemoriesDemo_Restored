using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MemoriesDemo.Helpers
{
    public partial class NavigationManager
    {
        private static NavigationManager _instance;

        public static NavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NavigationManager();
                }
                return _instance;
            }
        }

        private NavigationManager()
        {
        }

        public Panel RootPanel { get; set; }

        public bool CanGoBack
        {
            get { return _navigationStack.Count > 1; }
        }

        public event EventHandler TransitionFired;

        private List<INavigationElement> _elements = new List<INavigationElement>();
        private Stack<INavigationElement> _navigationStack = new Stack<INavigationElement>();
        public INavigationElement _curNavElement;

        public void AddNavigationElement(INavigationElement element)
        {
            _elements.Add(element);
            var uiElement = element as UIElement;
            uiElement.IsHitTestVisible = false;
            uiElement.Visibility = Visibility.Collapsed;
            RootPanel.Children.Add(uiElement);
        }

        public void TransitionElement(INavigationElement fromElement, string uriString)
        {
            INavigationElement target = null;
            foreach (var element in _elements)
            {
                if (element.GetUriString() == uriString)
                {
                    target = element;
                    break;
                }
            }

            if (target != null)
            {
                TransitionElement(fromElement, target);
            }
        }

        public void TransitionElement(INavigationElement fromElement, INavigationElement toElement)
        {
            var fromUI = fromElement as UIElement;
            var toUI = toElement as UIElement;

            if (fromElement == null && _curNavElement != null)
            {
                _curNavElement.TransitionOutComplete += OnTransitionOutComplete;
                _curNavElement.TransitionOut();
            }
            else if (ContainsElement(RootPanel.Children, fromUI))
            {
                fromUI.IsHitTestVisible = false;
                fromElement.TransitionOutComplete += OnTransitionOutComplete;
                fromElement.TransitionOut();
            }

            if (ContainsElement(RootPanel.Children, toUI))
            {
                if (!_navigationStack.Contains(toElement))
                {
                    _navigationStack.Push(toElement);
                }
                else
                {
                    INavigationElement popped = null;
                    while (popped != toElement && _navigationStack.Count > 0)
                    {
                        popped = _navigationStack.Pop();
                    }
                }

                toElement.TransitionInComplete += OnTransitionInComplete;
                toUI.Visibility = Visibility.Visible;
                toElement.TransitionIn();
                _curNavElement = toElement;

                if (TransitionFired != null)
                {
                    TransitionFired.Invoke(this, null);
                }
            }
        }

        public void GoBack()
        {
            var current = _navigationStack.Pop();
            INavigationElement previous = null;

            if (_navigationStack.Count > 0)
            {
                previous = _navigationStack.Pop();
            }

            TransitionElement(current, previous);
        }

        private void OnTransitionOutComplete(object sender, object e)
        {
            var element = sender as INavigationElement;
            if (element != null)
            {
                element.TransitionOutComplete -= OnTransitionOutComplete;
            }
            var uiElement = sender as UIElement;
            if (uiElement != null)
            {
                uiElement.IsHitTestVisible = false;
                uiElement.Visibility = Visibility.Collapsed;
            }
        }

        private void OnTransitionInComplete(object sender, object e)
        {
            var element = sender as INavigationElement;
            if (element != null)
            {
                element.TransitionInComplete -= OnTransitionInComplete;
            }
            var uiElement = sender as UIElement;
            if (uiElement != null)
            {
                uiElement.IsHitTestVisible = true;
            }
        }

        private bool ContainsElement(UIElementCollection collection, UIElement elem)
        {
            if (elem == null) return false;
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i] == elem)
                {
                    return true;
                }
            }
            return false;
        }
    }
}