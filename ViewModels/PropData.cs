// PropData.cs
namespace MemoriesDemo.ViewModels
{
    public partial class PropData
    {
        public PropData()
        {
            this.CompositeTransform = new CompositeTransformData();
            this.ImageAsset = "";
        }

        public string ImageAsset { get; set; }
        public CompositeTransformData CompositeTransform { get; set; }
        public double PanelCanvasWidth { get; set; }
        public double PanelCanvasHeight { get; set; }
    }
}