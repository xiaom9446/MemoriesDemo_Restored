using System.Collections.Generic;
using MemoriesDemo.ViewModels;
using MemoriesDemo.Views;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace MemoriesDemo.Helpers
{
    public interface IGenericAlbumPageView
    {
        Panel Panel_Img1 { get; }
        Panel Panel_Img2 { get; }
        Panel Panel_Img3 { get; }
        Panel Panel_Img4 { get; }
        Panel Panel_Img5 { get; }
        Panel Panel_Img6 { get; }
        Panel Panel_Img7 { get; }
        Panel Panel_Img8 { get; }

        Panel Tb_Desc1 { get; }
        Panel Tb_Desc2 { get; }
        Panel Tb_Desc3 { get; }
        Panel Tb_Desc4 { get; }
        Panel Tb_Desc5 { get; }
        Panel Tb_Desc6 { get; }
        Panel Tb_Desc7 { get; }
        Panel Tb_Desc8 { get; }

        bool IsEditing { get; set; }

        List<MediaPageItemView> MediaPageItems { get; }
        List<PropUserControl> PropViews { get; set; }
        List<TextPageItemView> TextPageItems { get; }
        PageVM PageVM { get; }

        int MaxItems { get; }
        int MaxTextItems { get; }

        Panel LayoutRoot { get; }
        Panel Canvas_LargeView { get; }

        AlbumPageTransitionInfo TransitionInfo { get; set; }
        Size DesiredViewSize { get; set; }

        Panel PropsPanel { get; }

        void OnExecuteMediaItem(object sender, ExecuteMediaEventArgs args);
        void OnAnimatedLargeViewIn_Completed(object sender, object e);
        void OnAnimatedLargeViewOut_Completed(object sender, object e);
        void OnPropUserControl_DeleteItem(object sender, object e);
        void OnUnloaded();
        void ShowAppBarMediaMenuItems(MediaPageItemView mediaPageItemView);
        void ClearDPs();
    }
}