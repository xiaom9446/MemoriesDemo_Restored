using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace MemoriesDemo.Helpers
{
    public partial class AlbumPageTransitionInfo
    {
        public int _fps { get; set; }
        public int _tickCounter { get; set; }

        public double _startXOffset;
        public double _startYOffset;
        public double _startWidth;
        public double _startHeight;

        public double _origXOffset;
        public double _origYOffset;
        public double _origWidth;
        public double _origHeight;

        public double _endWidth;
        public double _endHeight;

        public bool _isLargeView;
        public bool _isInTransitionAnimation;

        public Storyboard _sb_LargeViewIn;
        public Storyboard _sb_LargeViewOut;
        public Panel _subPanel_LargeView;
    }
}