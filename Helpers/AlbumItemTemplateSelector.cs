using MemoriesDemo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MemoriesDemo.Helpers
{
    public partial class AlbumItemTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null)
            {
                var album = item as AlbumVM;
                if (album != null)
                {
                    // MainPage._s_Instance 是 MainPage 的单例，FindName 查找 XAML 中
                    // 以 TemplateID 为 x:Name 的 DataTemplate 资源
                    return MainPage._s_Instance.FindName(album.TemplateID) as DataTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}