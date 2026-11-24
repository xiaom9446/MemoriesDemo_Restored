using System;
using Windows.UI.Xaml.Data;

namespace MemoriesDemo.Helpers
{
    /// <summary>
    /// 相册卡片底部黑色条里的时间显示，格式与原版一致：“December 2011”。
    /// </summary>
    public sealed class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
                return ((DateTime)value).ToString("MMMM yyyy");

            var dto = value as DateTimeOffset?;
            if (dto.HasValue)
                return dto.Value.ToString("MMMM yyyy");

            return value == null ? string.Empty : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
