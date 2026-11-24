using System;
using System.Collections.Generic;
using MemoriesDemo.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace MemoriesDemo.Helpers
{
    public partial class AlbumItemTemplateViewHelper
    {
        public static void InitComponent(IAlbumItemTemplateView view)
        {
            view.PanelTiles = new List<Panel>
            {
                view.Panel_Tile0,
                view.Panel_Tile1,
                view.Panel_Tile2,
                view.Panel_Tile3,
                view.Panel_Tile4,
                view.Panel_Tile5,
                view.Panel_Tile6
            };
            view.Panel_Large.Opacity = 0.0;
            view._random = new Random((int)DateTime.Now.Ticks);
            view._nextAnimationIndex = -1;

            view.Btn_Delete.Click += view.OnBtn_Delete_Click;

            var fe = view as FrameworkElement;
            if (fe != null)
            {
                fe.Unloaded += (s, e) => view.ClearDPs();
            }

            view._elementOutsidePopup = new Canvas
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Opacity = 0.4,
                Width = 1376.0,
                Height = 800.0
            };
            view._elementOutsidePopup.PointerPressed += view.OnElementOutsidePopup_PointerPressed;

            var canvas = new Canvas();
            view.PopUp.Child = canvas;
            canvas.Children.Add(view._elementOutsidePopup);
            canvas.Children.Add(view.PopupChild);
        }

        public static void Init(IAlbumItemTemplateView view, AlbumVM albumVM)
        {
            view._albumVM = albumVM;

            var uc = view as UserControl;
            if (uc != null)
            {
                uc.Holding += view.OnAlbumItemTemplateView_Holding;
                uc.RightTapped += view.OnAlbumItemTemplateView_RightTapped;
            }

            if (!string.IsNullOrEmpty(view._albumVM.Image))
            {
                var bmp = new BitmapImage
                {
                    UriSource = new Uri(MainPage._s_Instance.BaseUri, view._albumVM.Image),
                    DecodePixelHeight = 500,
                    DecodePixelWidth = 500
                };
                var img = new Image { Stretch = Stretch.Uniform, Source = bmp };
                view.Panel_Large.Children.Clear();
                view.Panel_Large.Children.Add(img);
            }

            view._albumVM.RefreshMediaList();
            view._mediaItems = view._albumVM.AllMediaItems;
            if (view._mediaItems.Count <= 0)
                return;

            int[] tileIndexes;
            int textTileIndex;
            GetTileIndexes(view, out tileIndexes, out textTileIndex);

            if (view._albumVM.Pages.Count > 0)
            {
                var page = view._albumVM.Pages[0] as PageVM;
                if (page != null && page.Captions.Count > 0 && !string.IsNullOrEmpty(page.Captions[0].Caption))
                {
                    var tb = new TextBlock { Text = page.Captions[0].Caption };
                    if (view._InnerTextBlockStyle_1 != null)
                        tb.Style = view._InnerTextBlockStyle_1;
                    if (textTileIndex >= 0 && textTileIndex < view.PanelTiles.Count)
                    {
                        view.PanelTiles[textTileIndex].Children.Clear();
                        view.PanelTiles[textTileIndex].Children.Add(tb);
                    }
                }
            }

            int mediaIdx = 0;
            for (int i = 0; i < tileIndexes.Length; i++)
            {
                if (mediaIdx >= view._mediaItems.Count)
                    return;

                var media = view._mediaItems[mediaIdx++] as MediaItemVM;
                while (media != null && Helper.IsMediaTypeOfVideo(media.MediaSource))
                {
                    if (mediaIdx >= view._mediaItems.Count) break;
                    media = view._mediaItems[mediaIdx++] as MediaItemVM;
                }
                if (media == null || string.IsNullOrEmpty(media.MediaSource))
                    continue;

                int targetTile = i;
                if (i >= tileIndexes.Length) targetTile = i - 1;

                if (media.IsMediaFromStream)
                {
                    try
                    {
                        var cd = Window.Current.Dispatcher;
                        var capturedMedia = media;
                        int capturedTile = tileIndexes[targetTile];
                        FileHelper.ReadFileStream(media.MediaSource, (sf, stream) =>
                        {
                            cd.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                // 关键：从详情页返回主页后，卡片要异步读图才出画面。
                                // 如果这时用户又点进相册，主页连同这些卡片已经被销毁，
                                // 回调仍然会到达 —— 此时往已脱离树的面板里塞 Image，
                                // 会让 XAML 去操作一个死掉的元素，直接崩（即“图片没加载
                                // 出来就点相册就闪退”）。所以先确认还停在主页。
                                if (MainViewModel.Instance.CurrentView != View.AlbumsView)
                                {
                                    try { stream.Dispose(); } catch { }
                                    return;
                                }

                                if (view.PanelTiles == null || capturedTile < 0 ||
                                    capturedTile >= view.PanelTiles.Count || view.PanelTiles[capturedTile] == null)
                                {
                                    try { stream.Dispose(); } catch { }
                                    return;
                                }

                                // 【崩溃现场】这段原来是无参 new BitmapImage() 直接 SetSource，
                                // 等于按照片原始分辨率同步解码。dump 已证实：就是这里
                                // 向 XAML 申请 89MB 像素缓冲失败，触发
                                // XAMLTerminateProcessOnMemoryExhaustion（0xC00001AD）。
                                // 卡片 tile 屏幕上只有一百多像素，按 320 解码完全够看。
                                var bmp = new BitmapImage { DecodePixelWidth = 320 };
                                bmp.SetSource(stream);
                                try { stream.Dispose(); } catch { }

                                var img = new Image { Stretch = Stretch.Uniform, Source = bmp };
                                view.PanelTiles[capturedTile].Children.Clear();
                                view.PanelTiles[capturedTile].Children.Add(img);
                            });
                        });
                    }
                    catch { }
                }
                else
                {
                    var bmp = new BitmapImage
                    {
                        DecodePixelWidth = 320,
                        UriSource = new Uri(MainPage._s_Instance.BaseUri, media.MediaSource)
                    };
                    var img = new Image { Stretch = Stretch.Uniform, Source = bmp };
                    view.PanelTiles[tileIndexes[targetTile]].Children.Clear();
                    view.PanelTiles[tileIndexes[targetTile]].Children.Add(img);
                }
            }
        }

        public static void ShowPopup(IAlbumItemTemplateView view)
        {
            view._albumVM.OnShowAlbumMenu(view);
            VisualStateManager.GoToState(view as UserControl, "Editing", true);
        }

        public static void HidePopup(IAlbumItemTemplateView view)
        {
            view.PopUp.IsOpen = false;
            VisualStateManager.GoToState(view as UserControl, "NotEditing", true);
        }

        public static void GetTileIndexes(IAlbumItemTemplateView view, out int[] tileIndexes, out int textTileIndex)
        {
            tileIndexes = null;
            textTileIndex = 0;

            if (view._albumVM.TemplateID == "AlbumItemTemplateView")
            {
                tileIndexes = new[] { 0, 3, 4, 5 };
                textTileIndex = 2;
                return;
            }
            if (view._albumVM.TemplateID == "AlbumItemTemplateView2")
            {
                tileIndexes = new[] { 0, 1, 2, 5 };
                textTileIndex = 6;
                return;
            }
            if (view._albumVM.TemplateID == "AlbumItemTemplateView3")
            {
                tileIndexes = new[] { 0, 1, 3, 4 };
                textTileIndex = 2;
                return;
            }
            if (view._albumVM.TemplateID == "AlbumItemTemplateView4")
            {
                tileIndexes = new[] { 1, 3, 5, 6 };
                textTileIndex = 0;
                return;
            }
            tileIndexes = new[] { 0, 3, 4, 5 };
            textTileIndex = 2;
        }

        public static string SelectRandomMedia(IAlbumItemTemplateView view)
        {
            if (view._albumVM != null && view._mediaItems != null && view._mediaItems.Count > 0)
            {
                int idx = view._random.Next(0, view._mediaItems.Count);
                var media = view._mediaItems[idx] as MediaItemVM;
                if (media != null)
                    return media.MediaSource;
            }
            return null;
        }

        public static void CreateDisplayAnimations(IAlbumItemTemplateView view)
        {
            if (view._albumVM == null) return;

            view.Sb_DisplayAnimation = new Storyboard();
            view.Sb_DisplayAnimation.Completed += view._sb_DisplayAnimation_Completed;

            int beginMs = 1000;
            int durationMs = 3500;

            if (view._isLargeImageDisplayed)
            {
                view._isLargeImageDisplayed = false;
                if (view.Panel_Large != null && view.Panel_Large.Children.Count > 0)
                {
                    var img = view.Panel_Large.Children[0] as Image;
                    if (img != null)
                    {
                        var anim = new DoubleAnimation
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                            From = 1.0,
                            To = 0.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
                            BeginTime = TimeSpan.FromMilliseconds(beginMs)
                        };
                        Storyboard.SetTarget(anim, view.Panel_Large);
                        Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                        view.Sb_DisplayAnimation.Children.Add(anim);
                    }
                }
            }
            else
            {
                int[] tileIndexes;
                int textTileIndex;
                GetTileIndexes(view, out tileIndexes, out textTileIndex);
                int pick = view._random.Next(0, tileIndexes.Length);
                view._nextAnimationIndex = tileIndexes[pick];

                Image img = null;
                if (view._nextAnimationIndex >= 0 && view._nextAnimationIndex < view.PanelTiles.Count)
                {
                    var panel = view.PanelTiles[view._nextAnimationIndex];
                    if (panel != null && panel.Children.Count > 0)
                        img = panel.Children[0] as Image;
                }

                if (img != null)
                {
                    var anim = new DoubleAnimation
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                        From = 1.0,
                        To = 0.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
                        BeginTime = TimeSpan.FromMilliseconds(beginMs)
                    };
                    Storyboard.SetTarget(anim, img);
                    Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                    view.Sb_DisplayAnimation.Children.Add(anim);
                }
            }

            view.Sb_DisplayAnimation.Begin();
        }

        public static void CreateLargeDisplayAnimation(IAlbumItemTemplateView view)
        {
            view.Sb_DisplayAnimation = new Storyboard();
            view.Sb_DisplayAnimation.Completed += view._sb_DisplayAnimationLarge_Completed;

            int beginMs = 1000;
            int durationMs = 3500;

            if (!view._isLargeImageDisplayed)
            {
                view._isLargeImageDisplayed = true;
                view.Panel_Large.Opacity = 0.0;

                var anim = new DoubleAnimation
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
                    BeginTime = TimeSpan.FromMilliseconds(beginMs)
                };
                Storyboard.SetTarget(anim, view.Panel_Pattern);
                Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                view.Sb_DisplayAnimation.Children.Add(anim);
            }
            else if (view.Panel_Large != null && view.Panel_Large.Children.Count > 0)
            {
                var img = view.Panel_Large.Children[0] as Image;
                if (img != null)
                {
                    var anim = new DoubleAnimation
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                        From = 1.0,
                        To = 0.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
                        BeginTime = TimeSpan.FromMilliseconds(beginMs)
                    };
                    Storyboard.SetTarget(anim, view.Panel_Large);
                    Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                    view.Sb_DisplayAnimation.Children.Add(anim);
                }
            }

            view.Sb_DisplayAnimation.Begin();
        }

        public static void DisplayAnimation_CompletedHelper(IAlbumItemTemplateView view, object sender, object e)
        {
            view.Sb_DisplayAnimation.Completed -= view._sb_DisplayAnimation_Completed;

            view.Sb_DisplayAnimation2 = new Storyboard();
            int beginMs = 1000;

            if (view.Panel_Pattern.Opacity == 0.0)
            {
                var anim = new DoubleAnimation
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(4000.0)),
                    BeginTime = TimeSpan.FromMilliseconds(beginMs)
                };
                Storyboard.SetTarget(anim, view.Panel_Pattern);
                Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                view.Sb_DisplayAnimation2.Children.Add(anim);
            }
            else if (view._nextAnimationIndex != -1 && view._nextAnimationIndex < view.PanelTiles.Count)
            {
                string source = SelectRandomMedia(view);
                if (!string.IsNullOrEmpty(source))
                {
                    var panel = view.PanelTiles[view._nextAnimationIndex];
                    if (panel != null && panel.Children.Count > 0)
                    {
                        var img = panel.Children[0] as Image;
                        if (img != null)
                        {
                            img.Source = new BitmapImage
                            {
                                DecodePixelWidth = 320,
                                UriSource = new Uri(MainPage._s_Instance.BaseUri, source)
                            };
                            var anim = new DoubleAnimation
                            {
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                                From = 0.0,
                                To = 1.0,
                                Duration = new Duration(TimeSpan.FromMilliseconds(4000.0)),
                                BeginTime = TimeSpan.FromMilliseconds(beginMs)
                            };
                            Storyboard.SetTarget(anim, img);
                            Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                            view.Sb_DisplayAnimation2.Children.Add(anim);
                        }
                    }
                }
            }

            view.Sb_DisplayAnimation2.Completed += (s, e2) => view._albumVM.OnDisplayAnimationCompleted();
            view.Sb_DisplayAnimation2.Begin();
            view._isLargeImageDisplayed = false;
        }

        public static void DisplayAnimationLarge_CompletedHelper(IAlbumItemTemplateView view, object sender, object e)
        {
            view.Sb_DisplayAnimation.Completed -= view._sb_DisplayAnimationLarge_Completed;

            view.Sb_DisplayAnimation2 = new Storyboard();
            string source = SelectRandomMedia(view);
            if (!string.IsNullOrEmpty(source) && view.Panel_Large != null && view.Panel_Large.Children.Count > 0)
            {
                var img = view.Panel_Large.Children[0] as Image;
                if (img != null)
                {
                    img.Source = new BitmapImage
                    {
                        DecodePixelWidth = 480,
                        UriSource = new Uri(MainPage._s_Instance.BaseUri, source)
                    };
                    var anim = new DoubleAnimation
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
                        From = 0.0,
                        To = 1.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(4000.0))
                    };
                    Storyboard.SetTarget(anim, view.Panel_Large);
                    Storyboard.SetTargetProperty(anim, "(UIElement.Opacity)");
                    view.Sb_DisplayAnimation2.Children.Add(anim);
                }
            }

            view.Sb_DisplayAnimation2.Completed += (s, e2) => view._albumVM.OnDisplayAnimationCompleted();
            view.Sb_DisplayAnimation2.Begin();
            view._isLargeImageDisplayed = true;
        }
    }
}