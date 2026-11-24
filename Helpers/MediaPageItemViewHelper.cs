using System;
using System.Collections.Generic;
using MemoriesDemo.ViewModels;
using MemoriesDemo.Views;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace MemoriesDemo.Helpers
{
    public partial class MediaPageItemViewHelper
    {
        public static void UpdateMedia(MediaItemVM mediaItemVM, Panel panel_Media, Panel layoutRoot, Action<object> setupPropsCompleted = null)
        {
            if (layoutRoot != null)
            {
                Size size = default(Size);
                size.Width = double.IsNaN(layoutRoot.Width) ? layoutRoot.ActualWidth : layoutRoot.Width;
                size.Height = double.IsNaN(layoutRoot.Height) ? layoutRoot.ActualHeight : layoutRoot.Height;
                UpdateMedia(mediaItemVM, panel_Media, true, new Size?(size), setupPropsCompleted);
                return;
            }
            UpdateMedia(mediaItemVM, panel_Media, false, default(Size?), setupPropsCompleted);
        }

        public static void UpdateMedia(MediaItemVM mediaItemVM, Panel panel_Media, bool useLayoutSize, Size? layoutSize, Action<object> setupPropsCompleted = null)
        {
            if (mediaItemVM.IsMediaFromStream)
            {
                var cd = Window.Current.Dispatcher;
                FileHelper.ReadFileStream(mediaItemVM.MediaSource, (sf, stream) =>
                {
                    cd.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        UpdateMedia(mediaItemVM, panel_Media, sf, stream, useLayoutSize, layoutSize, setupPropsCompleted);
                    });
                });
                return;
            }

            if (!string.IsNullOrEmpty(mediaItemVM.MediaSource))
            {
                panel_Media.Children.Clear();

                if (Helper.IsMediaTypeOfVideo(mediaItemVM.MediaSource))
                {
                    var mediaElement = new MediaElement
                    {
                        Source = new Uri(MainPage._s_Instance.BaseUri, mediaItemVM.MediaSource),
                        AutoPlay = false
                    };
                    if (useLayoutSize && layoutSize != null)
                    {
                        mediaElement.Width = layoutSize.Value.Width;
                        mediaElement.Height = layoutSize.Value.Height;
                    }
                    panel_Media.Children.Add(mediaElement);
                }
                else
                {
                    var bmp = new BitmapImage
                    {
                        DecodePixelWidth = ComputeDecodeWidth(useLayoutSize, layoutSize),
                        UriSource = new Uri(MainPage._s_Instance.BaseUri, mediaItemVM.MediaSource)
                    };
                    var img = new Image
                    {
                        Stretch = Stretch.Uniform,
                        Source = bmp
                    };
                    if (useLayoutSize && layoutSize != null)
                    {
                        img.Width = layoutSize.Value.Width;
                        img.Height = layoutSize.Value.Height;
                    }
                    panel_Media.Children.Add(img);
                }

                SetupProps(mediaItemVM, panel_Media, setupPropsCompleted);
            }
        }

        public static void UpdateMedia(MediaItemVM mediaItemVM, Panel panel_Media, StorageFile storageFile, IRandomAccessStream stream, Panel layoutRoot, Action<object> setupPropsCompleted = null)
        {
            if (layoutRoot != null)
            {
                Size size = default(Size);
                size.Width = double.IsNaN(layoutRoot.Width) ? layoutRoot.ActualWidth : layoutRoot.Width;
                size.Height = double.IsNaN(layoutRoot.Height) ? layoutRoot.ActualHeight : layoutRoot.Height;
                UpdateMedia(mediaItemVM, panel_Media, storageFile, stream, true, new Size?(size), setupPropsCompleted);
                return;
            }
            UpdateMedia(mediaItemVM, panel_Media, storageFile, stream, false, default(Size?), setupPropsCompleted);
        }

        public static void UpdateMedia(MediaItemVM mediaItemVM, Panel panel_Media, StorageFile storageFile, IRandomAccessStream stream, bool useLayoutSize, Size? layoutSize, Action<object> setupPropsCompleted = null)
        {
            panel_Media.Children.Clear();

            if (Helper.IsMediaTypeOfVideo(mediaItemVM.MediaSource))
            {
                var mediaElement = new MediaElement { AutoPlay = false };
                if (useLayoutSize && layoutSize != null)
                {
                    mediaElement.Width = layoutSize.Value.Width;
                    mediaElement.Height = layoutSize.Value.Height;
                }
                else
                {
                    mediaElement.Width = 700.0;
                    mediaElement.Height = 700.0;
                }
                panel_Media.Children.Add(mediaElement);
                mediaElement.SetSource(stream, storageFile.ContentType);
            }
            else
            {
                // 关键修改：SetSource 是同步解码，XAML 必须先一次性分配
                // width*height*4 的完整像素缓冲，才能出图。
                // 不指定 DecodePixelWidth 时按照片原始分辨率解码，
                // 一张 2000 万像素的手机照就要 89MB —— 32 位进程只有 2~4GB
                // 地址空间，几张就耗尽，XAML 会直接 TerminateProcess（0xC00001AD）。
                // 这里按实际显示宽度解码，内存降到 1/100 以下。
                var bmp = new BitmapImage
                {
                    DecodePixelWidth = ComputeDecodeWidth(useLayoutSize, layoutSize)
                };
                bmp.SetSource(stream);
                try { stream.Dispose(); } catch { }

                var img = new Image
                {
                    Stretch = Stretch.Uniform,
                    Source = bmp
                };
                if (useLayoutSize && layoutSize != null)
                {
                    img.Width = layoutSize.Value.Width;
                    img.Height = layoutSize.Value.Height;
                }
                else
                {
                    img.Width = 700.0;
                    img.Height = 700.0;
                }
                panel_Media.Children.Add(img);
            }

            SetupProps(mediaItemVM, panel_Media, setupPropsCompleted);
        }

        /// <summary>
        /// 按实际显示宽度计算解码尺寸，避免全分辨率解码吃光内存。
        /// 上限 1400px：详情页图片是全屏背景，1366 宽的屏足够清晰；
        /// 下限 256px：防止布局尚未完成时算出 0 导致解码出 1px 的图。
        /// </summary>
        private static int ComputeDecodeWidth(bool useLayoutSize, Size? layoutSize)
        {
            double w = 1024.0;

            if (useLayoutSize && layoutSize != null && layoutSize.Value.Width > 0.0)
            {
                w = layoutSize.Value.Width;
            }

            int px = (int)Math.Round(w);
            if (px < 256) px = 256;
            if (px > 1400) px = 1400;
            return px;
        }

        public static void SetupProps(MediaItemVM mediaItemVM, Panel panel_Media, Action<object> setupPropsCompleted = null)
        {
            if (mediaItemVM.Props != null && mediaItemVM.Props.Count > 0)
            {
                if (panel_Media.ActualWidth == 0.0 || panel_Media.ActualHeight == 0.0)
                {
                    RoutedEventHandler handler = null;
                    handler = (s, e) =>
                    {
                        panel_Media.Loaded -= handler;
                        var sb = new Storyboard
                        {
                            Duration = new Duration(TimeSpan.FromMilliseconds(1000.0))
                        };
                        sb.Completed += (s2, e2) =>
                        {
                            SetupPropsHelper(mediaItemVM, panel_Media, setupPropsCompleted);
                        };
                        sb.Begin();
                    };
                    panel_Media.Loaded += handler;
                    return;
                }
                SetupPropsHelper(mediaItemVM, panel_Media, setupPropsCompleted);
            }
        }

        private static void SetupPropsHelper(MediaItemVM mediaItemVM, Panel panel_Media, Action<object> setupPropsCompleted = null)
        {
            if (mediaItemVM.Props != null && mediaItemVM.Props.Count > 0 && panel_Media.ActualWidth > 0.0 && panel_Media.ActualHeight > 0.0)
            {
                var list = new List<UIElement>();
                foreach (var propData in mediaItemVM.Props)
                {
                    var prop = new PropUserControl();
                    prop.Init(panel_Media.ActualWidth, panel_Media.ActualHeight, propData);
                    panel_Media.Children.Add(prop);
                    list.Add(prop);
                }
                if (setupPropsCompleted != null)
                {
                    setupPropsCompleted.Invoke(list);
                }
            }
        }

        public static void SetupProps(PageVM pageVM, Panel panel_Media, Action<object> setupPropsCompleted = null)
        {
            if (pageVM.Props != null && pageVM.Props.Count > 0)
            {
                if (panel_Media.ActualWidth == 0.0 || panel_Media.ActualHeight == 0.0)
                {
                    RoutedEventHandler handler = null;
                    handler = (s, e) =>
                    {
                        panel_Media.Loaded -= handler;
                        var sb = new Storyboard
                        {
                            Duration = new Duration(TimeSpan.FromMilliseconds(1000.0))
                        };
                        sb.Completed += (s2, e2) =>
                        {
                            SetupPropsHelper(pageVM, panel_Media, setupPropsCompleted);
                        };
                        sb.Begin();
                    };
                    panel_Media.Loaded += handler;
                    return;
                }
                SetupPropsHelper(pageVM, panel_Media, setupPropsCompleted);
            }
        }

        private static void SetupPropsHelper(PageVM pageVM, Panel panel_Media, Action<object> setupPropsCompleted = null)
        {
            if (pageVM.Props != null && pageVM.Props.Count > 0 && panel_Media.ActualWidth > 0.0 && panel_Media.ActualHeight > 0.0)
            {
                var list = new List<UIElement>();
                foreach (var obj in pageVM.Props)
                {
                    var prop = new PropUserControl();
                    prop.Init(panel_Media.ActualWidth, panel_Media.ActualHeight, obj as PropData);
                    panel_Media.Children.Add(prop);
                    list.Add(prop);
                }
                if (setupPropsCompleted != null)
                {
                    setupPropsCompleted.Invoke(list);
                }
            }
        }
    }
}