using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MemoriesDemo.Helpers
{
    public partial class AlbumPageTemplateSelector_REORDER : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null)
            {
                var page = item as PageVM;
                if (page != null)
                {
                    return MainPage._s_Instance.FindName(page.PageID + "_REORDER") as DataTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}