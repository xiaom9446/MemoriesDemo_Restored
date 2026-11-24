using Windows.Foundation.Collections;

namespace MemoriesDemo.Helpers
{
    public partial class CustomVectorChangedEventArgs : IVectorChangedEventArgs
    {
        public CollectionChange CollectionChange { get; set; }
        public uint Index { get; set; }
    }
}