using System;
using System.Collections.Generic;
using MemoriesDemo.ViewModels;
using MemoriesDemo.Views;
using MemoriesDemo.ViewTemplates;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MemoriesDemo.Helpers
{
    public partial class Helper
    {
        public static T GetVisualChild<T>(DependencyObject parent, int index) where T : DependencyObject
        {
            T result = default(T);
            int count = 0;
            Queue<DependencyObject> queue = new Queue<DependencyObject>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                DependencyObject current = queue.Dequeue();
                result = current as T;
                if (result != null)
                {
                    if (count == index)
                        break;
                    count++;
                }
                else
                {
                    int childCount = VisualTreeHelper.GetChildrenCount(current);
                    for (int i = 0; i < childCount; i++)
                    {
                        queue.Enqueue(VisualTreeHelper.GetChild(current, i));
                    }
                }
            }
            return result;
        }

        public static IAlbumItemTemplateView GetAlbumItemTemplateViewFromID(string templateID)
        {
            switch (templateID)
            {
                case "AlbumItemTemplateView1": return new AlbumItemTemplateView();
                case "AlbumItemTemplateView2": return new AlbumItemTemplateView2();
                case "AlbumItemTemplateView3": return new AlbumItemTemplateView3();
                case "AlbumItemTemplateView4": return new AlbumItemTemplateView4();
                default: return new AlbumItemTemplateView();
            }
        }

        public static void ScrollIntoView(ItemsControl ic, Panel scrollHost, bool isVertical, int index)
        {
            Rect scrollHostRect = new Rect();
            Rect containerRect = new Rect();

            if (IsOnCurrentPage(ic, index, isVertical, scrollHost, ref scrollHostRect, ref containerRect))
                return;

            var scrollViewer = GetVisualChild<ScrollViewer>(ic, 0);
            if (scrollViewer == null)
                return;

            if (isVertical)
                return;

            double offset = scrollViewer.HorizontalOffset;
            double delta = 0.0;
            double containerRight = containerRect.X + containerRect.Width;
            double hostRight = scrollHostRect.X + scrollHostRect.Width;

            if (containerRight < hostRight)
            {
                delta = hostRight - containerRight;
                offset += delta;
            }

            if (scrollHostRect.X - delta < containerRect.X)
            {
                offset -= containerRect.X - (scrollHostRect.X - delta);
            }

            scrollViewer.ScrollToHorizontalOffset(offset);
        }

        public static bool IsOnCurrentPage(ItemsControl ic, int index, bool isVertical, Panel scrollHost, ref Rect scrollHostRect, ref Rect containerRect)
        {
            if (ic == null)
                return false;
            if (scrollHost == null)
                return false;

            var scrollViewer = GetVisualChild<ScrollViewer>(ic, 0);

            scrollHostRect.X = 0.0;
            scrollHostRect.Y = 0.0;
            scrollHostRect.Width = scrollHost.ActualWidth;
            scrollHostRect.Height = scrollHost.ActualHeight;

            var container = ic.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            if (container == null)
                return false;

            double width = container.ActualWidth;
            double height = container.ActualHeight;

            if (container is FlipViewItem)
            {
                if (scrollViewer != null)
                {
                    width = scrollViewer.ActualWidth;
                    height = scrollViewer.ActualHeight;
                }
            }

            var transform = container.TransformToVisual(scrollHost);
            Point topLeft = transform.TransformPoint(default(Point));
            Point bottomRight = transform.TransformPoint(new Point(width, height));

            containerRect.X = Math.Min(topLeft.X, bottomRight.X);
            containerRect.Y = Math.Min(topLeft.Y, bottomRight.Y);
            containerRect.Width = Math.Max(Math.Max(topLeft.X, bottomRight.X) - containerRect.X, 0.0);
            containerRect.Height = Math.Max(Math.Max(topLeft.Y, bottomRight.Y) - containerRect.Y, 0.0);

            if (isVertical)
            {
                return scrollHostRect.Y <= containerRect.Y
                    && containerRect.Y + containerRect.Height <= scrollHostRect.Y + scrollHostRect.Height;
            }
            else
            {
                return scrollHostRect.X <= containerRect.X
                    && containerRect.X + containerRect.Width <= scrollHostRect.X + scrollHostRect.Width;
            }
        }

        public static string GetRandomPageID(out int maxItems)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            string templateID = "PageTemplate_Scrap_1";
            maxItems = 1;

            int num = random.Next(0, 8);
            switch (num)
            {
                case 0: templateID = "PageTemplate_Scrap_1"; maxItems = 1; break;
                case 1: templateID = "PageTemplate_Scrap_2"; maxItems = 1; break;
                case 2: templateID = "PageTemplate_Scrap_3"; maxItems = 1; break;
                case 3: templateID = "PageTemplate_Scrap_4"; maxItems = 1; break;
                case 4: templateID = "PageTemplate_Scrap_5"; maxItems = 3; break;
                case 5: templateID = "PageTemplate_Scrap_6"; maxItems = 1; break;
                case 6: templateID = "PageTemplate_Scrap_7"; maxItems = 1; break;
            }
            return templateID;
        }

        public static IGenericAlbumPageView GetViewFromTemplateID(string templateID)
        {
            switch (templateID)
            {
                case "PageTemplate_Scrap_1": return new PageTemplate_Scrap_1();
                case "PageTemplate_Scrap_2": return new PageTemplate_Scrap_2();
                case "PageTemplate_Scrap_3": return new PageTemplate_Scrap_3();
                case "PageTemplate_Scrap_4": return new PageTemplate_Scrap_4();
                case "PageTemplate_Scrap_5": return new PageTemplate_Scrap_5();
                case "PageTemplate_Scrap_6": return new PageTemplate_Scrap_6();
                case "PageTemplate_Scrap_7": return new PageTemplate_Scrap_7();
                case "PageTemplate_Scrap_8": return new PageTemplate_Scrap_8();
                case "PageTemplate_Basic_1": return new PageTemplate_Basic_1();
                case "PageTemplate_Basic_2": return new PageTemplate_Basic_2();
                case "PageTemplate_Basic_3": return new PageTemplate_Basic_3();
                case "PageTemplate_Basic_4": return new PageTemplate_Basic_4();
                case "PageTemplate_Basic_5": return new PageTemplate_Basic_5();
                case "PageTemplate_Basic_6": return new PageTemplate_Basic_6();
                case "PageTemplate_Basic_7": return new PageTemplate_Basic_7();
                default: return new PageTemplate_Scrap_7();
            }
        }

        public static CompositeTransform CompositeTransformFromCTData(CompositeTransformData ctData)
        {
            return new CompositeTransform
            {
                TranslateX = ctData.TranslateX,
                TranslateY = ctData.TranslateY,
                ScaleX = ctData.ScaleX,
                ScaleY = ctData.ScaleY,
                SkewX = ctData.SkewX,
                SkewY = ctData.SkewY,
                Rotation = ctData.Rotation
            };
        }

        public static CompositeTransformData CTDataFromCompositeTransform(CompositeTransform transform)
        {
            return new CompositeTransformData
            {
                TranslateX = transform.TranslateX,
                TranslateY = transform.TranslateY,
                ScaleX = transform.ScaleX,
                ScaleY = transform.ScaleY,
                SkewX = transform.SkewX,
                SkewY = transform.SkewY,
                Rotation = transform.Rotation
            };
        }

        public static bool IsMediaTypeOfVideo(string mediaSource)
        {
            return !string.IsNullOrEmpty(mediaSource)
                && (mediaSource.EndsWith(".wmv") || mediaSource.EndsWith(".mp4"));
        }
    }
}