// CompositeTransformData.cs
namespace MemoriesDemo.ViewModels
{
    public partial class CompositeTransformData
    {
        public CompositeTransformData()
        {
            this.ScaleX = 1.0;
            this.ScaleY = 1.0;
        }

        public double TranslateX { get; set; }
        public double TranslateY { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double SkewX { get; set; }
        public double SkewY { get; set; }
        public double Rotation { get; set; }
    }
}