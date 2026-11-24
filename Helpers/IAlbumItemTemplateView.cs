using System;
using System.Collections.Generic;
using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace MemoriesDemo.Helpers
{
    public interface IAlbumItemTemplateView
    {
        Panel Panel_Tile0 { get; set; }
        Panel Panel_Tile1 { get; set; }
        Panel Panel_Tile2 { get; set; }
        Panel Panel_Tile3 { get; set; }
        Panel Panel_Tile4 { get; set; }
        Panel Panel_Tile5 { get; set; }
        Panel Panel_Tile6 { get; set; }

        Panel Panel_Large { get; set; }
        Style _InnerTextBlockStyle_1 { get; set; }
        Panel Panel_Pattern { get; set; }

        Storyboard Sb_DisplayAnimation { get; set; }
        Storyboard Sb_DisplayAnimation2 { get; set; }

        Popup PopUp { get; set; }
        FrameworkElement PopupChild { get; set; }
        Button Btn_Delete { get; set; }

        List<Panel> PanelTiles { get; set; }
        Canvas _elementOutsidePopup { get; set; }
        Random _random { get; set; }
        AlbumVM _albumVM { get; set; }
        ObservableList _mediaItems { get; set; }
        int _nextAnimationIndex { get; set; }
        bool _isLargeImageDisplayed { get; set; }

        void ShowPopup();
        void OnBtn_Delete_Click(object sender, RoutedEventArgs e);
        void OnElementOutsidePopup_PointerPressed(object sender, PointerRoutedEventArgs e);
        void _sb_DisplayAnimation_Completed(object sender, object e);
        void _sb_DisplayAnimationLarge_Completed(object sender, object e);
        void OnAlbumItemTemplateView_RightTapped(object sender, RightTappedRoutedEventArgs e);
        void OnAlbumItemTemplateView_Holding(object sender, HoldingRoutedEventArgs e);
        void ClearDPs();
    }
}