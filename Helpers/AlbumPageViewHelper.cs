using System;
using System.Collections.Generic;
using MemoriesDemo.ViewModels;
using MemoriesDemo.Views;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace MemoriesDemo.Helpers
{
    public partial class AlbumPageViewHelper
    {
        public static void Init(IGenericAlbumPageView view, PageVM pageVM)
        {
            view.TransitionInfo = new AlbumPageTransitionInfo();

            var fe = view as FrameworkElement;
            if (fe != null)
            {
                fe.Unloaded += (s, e) => view.ClearDPs();
            }

            if (pageVM != null)
            {
                for (int i = 0; i < view.MaxItems; i++)
                {
                    MediaItemVM mediaItem = null;
                    foreach (var m in pageVM.MediaItems)
                    {
                        if (m.PageInnerIndex == i)
                        {
                            mediaItem = m;
                            break;
                        }
                    }
                    if (mediaItem == null)
                    {
                        mediaItem = new MediaItemVM { PageInnerIndex = i };
                        pageVM.MediaItems.Add(mediaItem);
                    }
                    CreateImage(view, mediaItem, i);
                }

                for (int j = 0; j < view.MaxTextItems; j++)
                {
                    if (j >= pageVM.Captions.Count)
                    {
                        pageVM.Captions.Add(new TextPageItemVM());
                    }
                    CreateTextPanel(view, pageVM.Captions[j], j);
                }

                MediaPageItemViewHelper.SetupProps(pageVM, view.PropsPanel, props =>
                {
                    foreach (UIElement el in (List<UIElement>)props)
                    {
                        var prop = el as PropUserControl;
                        view.PropViews.Add(prop);
                        prop.DeleteItem += view.OnPropUserControl_DeleteItem;
                    }
                });
            }
        }

        public static void CreateImage(IGenericAlbumPageView view, MediaItemVM mediaItem, int imageIndex)
        {
            var panel = GetImagePanelFromIndex(view, imageIndex);
            if (panel != null)
            {
                var itemView = new MediaPageItemView();
                itemView.Init(view, mediaItem, panel.Width, panel.Height);
                panel.Children.Add(itemView);
                view.MediaPageItems.Add(itemView);
                panel.Visibility = Visibility.Visible;
            }
        }

        public static void CreateTextPanel(IGenericAlbumPageView view, TextPageItemVM textItem, int textPanelIndex)
        {
            var panel = GetTextBlockCaptionFromIndex(view, textPanelIndex);
            if (panel != null)
            {
                var itemView = new TextPageItemView();
                if (panel.Tag != null)
                {
                    var tb = panel.Tag as TextBlock;
                    if (tb != null)
                    {
                        itemView.TextContent.Foreground = tb.Foreground;
                        itemView.TextContent.FontSize = tb.FontSize;
                        itemView.TextContent.Width = tb.Width;
                        itemView.TextContent.Height = tb.Height;
                        itemView.TextContent.TextWrapping = tb.TextWrapping;
                    }
                }
                itemView.DataContext = textItem;
                itemView.SetupView();
                panel.Children.Clear();
                panel.Children.Add(itemView);
                view.TextPageItems.Add(itemView);
                panel.Visibility = Visibility.Visible;
            }
        }

        public static void UpdateLayout(IGenericAlbumPageView view)
        {
            foreach (var item in view.MediaPageItems)
            {
                item.Width = view.DesiredViewSize.Width;
                item.Height = view.DesiredViewSize.Height;
            }
        }

        public static void OnUnloaded(IGenericAlbumPageView view)
        {
        }

        public static void BeginEdit(IGenericAlbumPageView view)
        {
            foreach (var item in view.MediaPageItems)
                item.BeginEdit();
            foreach (var item in view.TextPageItems)
                item.BeginEdit();
            foreach (var prop in view.PropViews)
                prop.BeginEdit();
        }

        public static void CompleteEdit(IGenericAlbumPageView view)
        {
            foreach (var item in view.MediaPageItems)
                item.CompleteEdit();
            foreach (var item in view.TextPageItems)
                item.CompleteEdit();

            int idx = 0;
            foreach (var prop in view.PropViews)
            {
                if (prop != null && idx < view.PageVM.Props.Count)
                {
                    var data = view.PageVM.Props[idx] as PropData;
                    prop.CompleteEdit();
                    if (data != null)
                    {
                        data.CompositeTransform = prop.PropData.CompositeTransform;
                        data.PanelCanvasWidth = prop.PropData.PanelCanvasWidth;
                        data.PanelCanvasHeight = prop.PropData.PanelCanvasHeight;
                    }
                }
                idx++;
            }
        }

        public static void SetIsReadOnly(IGenericAlbumPageView view, bool isReadOnly)
        {
            foreach (var item in view.MediaPageItems)
                item.IsReadOnly = isReadOnly;
            foreach (var item in view.TextPageItems)
                item.IsReadOnly = isReadOnly;
        }

        public static Panel GetImagePanelFromIndex(IGenericAlbumPageView view, int imageIndex)
        {
            switch (imageIndex)
            {
                case 0: return view.Panel_Img1;
                case 1: return view.Panel_Img2;
                case 2: return view.Panel_Img3;
                case 3: return view.Panel_Img4;
                case 4: return view.Panel_Img5;
                case 5: return view.Panel_Img6;
                case 6: return view.Panel_Img7;
                case 7: return view.Panel_Img8;
                default: return view.Panel_Img1;
            }
        }

        public static Panel GetTextBlockCaptionFromIndex(IGenericAlbumPageView view, int imageIndex)
        {
            switch (imageIndex)
            {
                case 0: return view.Tb_Desc1;
                case 1: return view.Tb_Desc2;
                case 2: return view.Tb_Desc3;
                case 3: return view.Tb_Desc4;
                case 4: return view.Tb_Desc5;
                case 5: return view.Tb_Desc6;
                case 6: return view.Tb_Desc7;
                case 7: return view.Tb_Desc8;
                default: return view.Tb_Desc1;
            }
        }

        public static void OnExecuteMediaItem(IGenericAlbumPageView view, object sender, ExecuteMediaEventArgs args)
        {
            var mediaItem = sender as MediaItemVM;
            if (mediaItem == null || string.IsNullOrEmpty(mediaItem.MediaSource) || view.TransitionInfo._isInTransitionAnimation)
                return;

            view.TransitionInfo._isLargeView = !view.TransitionInfo._isLargeView;
            view.TransitionInfo._isInTransitionAnimation = true;

            if (view.TransitionInfo._isLargeView)
            {
                view.TransitionInfo._subPanel_LargeView = new Grid
                {
                    Background = new SolidColorBrush(Colors.Transparent)
                };
                MediaPageItemViewHelper.UpdateMedia(mediaItem, view.TransitionInfo._subPanel_LargeView, view.LayoutRoot, null);
                view.Canvas_LargeView.Children.Clear();
                view.Canvas_LargeView.Children.Add(view.TransitionInfo._subPanel_LargeView);

                var fe = args.VisualElement as FrameworkElement;
                var transform = fe.TransformToVisual(view.Canvas_LargeView);
                var point = transform.TransformPoint(default(Point));
                Canvas.SetLeft(view.TransitionInfo._subPanel_LargeView, point.X);
                Canvas.SetTop(view.TransitionInfo._subPanel_LargeView, point.Y);
                view.TransitionInfo._subPanel_LargeView.Width = view.LayoutRoot.ActualWidth;
                view.TransitionInfo._subPanel_LargeView.Height = view.LayoutRoot.ActualHeight;

                view.Canvas_LargeView.Clip = new RectangleGeometry
                {
                    Rect = new Rect(point.X, point.Y, fe.ActualWidth, fe.ActualHeight)
                };

                view.TransitionInfo._origHeight = view.TransitionInfo._startHeight = fe.ActualHeight;
                view.TransitionInfo._origWidth = view.TransitionInfo._startWidth = fe.ActualWidth;
                view.TransitionInfo._endHeight = view.LayoutRoot.ActualHeight;
                view.TransitionInfo._endWidth = view.LayoutRoot.ActualWidth;
                view.TransitionInfo._origXOffset = view.TransitionInfo._startXOffset = point.X;
                view.TransitionInfo._origYOffset = view.TransitionInfo._startYOffset = point.Y;
                view.TransitionInfo._fps = 15;
                view.TransitionInfo._tickCounter = 0;

                view.Canvas_LargeView.Visibility = Visibility.Visible;
                view.Canvas_LargeView.Opacity = 1.0;
                view.LayoutRoot.Opacity = 0.0;

                view.TransitionInfo._sb_LargeViewIn = new Storyboard();
                view.TransitionInfo._sb_LargeViewIn.Completed += view.OnAnimatedLargeViewIn_Completed;
                view.TransitionInfo._sb_LargeViewIn.Begin();

                var sb = new Storyboard();
                var anim = new DoubleAnimation
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(100.0)),
                    BeginTime = TimeSpan.Zero
                };
                Storyboard.SetTarget(anim, view.LayoutRoot);
                Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                sb.Children.Add(anim);
                sb.Begin();
            }
            else
            {
                view.TransitionInfo._tickCounter = 0;
                view.TransitionInfo._sb_LargeViewOut = new Storyboard();
                view.TransitionInfo._sb_LargeViewOut.Completed += view.OnAnimatedLargeViewOut_Completed;
                view.TransitionInfo._sb_LargeViewOut.Begin();

                var sb = new Storyboard();
                var anim = new DoubleAnimation
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(100.0)),
                    BeginTime = TimeSpan.Zero
                };
                Storyboard.SetTarget(anim, view.LayoutRoot);
                Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                sb.Children.Add(anim);
                sb.Begin();
            }
        }

        public static void OnAnimatedLargeViewIn_Completed(IGenericAlbumPageView view, object sender, object e)
        {
            var info = view.TransitionInfo;

            if (info._tickCounter >= info._fps)
            {
                info._sb_LargeViewIn.Stop();
                info._sb_LargeViewIn.Completed -= view.OnAnimatedLargeViewIn_Completed;
                info._isInTransitionAnimation = false;
                return;
            }

            info._tickCounter++;
            double dx = Math.Abs(info._origWidth - info._endWidth);
            double dy = Math.Abs(info._origHeight - info._endHeight);
            info._startXOffset -= info._origXOffset / info._fps;
            info._startYOffset -= info._origYOffset / info._fps;
            info._startHeight += dy / info._fps;
            info._startWidth += dx / info._fps;

            info._startXOffset = Math.Max(0.0, info._startXOffset);
            info._startYOffset = Math.Max(0.0, info._startYOffset);
            info._startWidth = Math.Min(info._endWidth, info._startWidth);
            info._startHeight = Math.Min(info._endHeight, info._startHeight);

            Canvas.SetLeft(info._subPanel_LargeView, info._startXOffset);
            Canvas.SetTop(info._subPanel_LargeView, info._startYOffset);
            view.Canvas_LargeView.Clip = new RectangleGeometry
            {
                Rect = new Rect(info._startXOffset, info._startYOffset, info._startWidth, info._startHeight)
            };

            info._sb_LargeViewIn.Begin();
        }

        public static void OnAnimatedLargeViewOut_Completed(IGenericAlbumPageView view, object sender, object e)
        {
            var info = view.TransitionInfo;

            if (info._tickCounter >= info._fps)
            {
                info._sb_LargeViewOut.Stop();
                info._sb_LargeViewOut.Completed -= view.OnAnimatedLargeViewIn_Completed;

                var sb = new Storyboard();
                var anim = new DoubleAnimation
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(50.0)),
                    BeginTime = TimeSpan.Zero
                };
                Storyboard.SetTarget(anim, view.Canvas_LargeView);
                Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                sb.Children.Add(anim);
                sb.Completed += (s, e2) =>
                {
                    info._isInTransitionAnimation = false;
                    view.Canvas_LargeView.Children.Clear();
                    view.Canvas_LargeView.Visibility = Visibility.Collapsed;
                };
                sb.Begin();
                return;
            }

            info._tickCounter++;
            double dx = Math.Abs(info._origWidth - info._endWidth);
            double dy = Math.Abs(info._origHeight - info._endHeight);
            info._startXOffset += info._origXOffset / info._fps;
            info._startYOffset += info._origYOffset / info._fps;
            info._startHeight -= dy / info._fps;
            info._startWidth -= dx / info._fps;

            info._startXOffset = Math.Min(info._origXOffset, info._startXOffset);
            info._startYOffset = Math.Min(info._origYOffset, info._startYOffset);
            info._startWidth = Math.Max(info._origWidth, info._startWidth);
            info._startHeight = Math.Max(info._origHeight, info._startHeight);

            Canvas.SetLeft(info._subPanel_LargeView, info._startXOffset);
            Canvas.SetTop(info._subPanel_LargeView, info._startYOffset);
            view.Canvas_LargeView.Clip = new RectangleGeometry
            {
                Rect = new Rect(info._startXOffset, info._startYOffset, info._startWidth, info._startHeight)
            };

            info._sb_LargeViewOut.Begin();
        }

        public static void OnPropUserControl_DeleteItem(IGenericAlbumPageView view, object sender, object e)
        {
            var prop = sender as PropUserControl;
            if (prop == null) return;
            var data = prop.PropData;
            view.PageVM.Props.Remove(data);
            view.PropViews.Remove(prop);
            view.PropsPanel.Children.Remove(prop);
            prop.DeleteItem -= view.OnPropUserControl_DeleteItem;
        }

        public static void ShowAppBarMediaMenuItems(IGenericAlbumPageView view, MediaPageItemView mediaPageItemView)
        {
            view.PageVM.OnShowAppBarMediaMenuItems(mediaPageItemView);
        }
    }
}