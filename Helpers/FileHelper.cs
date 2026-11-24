using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MemoriesDemo.Helpers
{
    public partial class FileHelper
    {
        public static async void ReadFileStream(string path, Action<StorageFile, IRandomAccessStream> onGetStream)
        {
            try
            {
                var sf = await StorageFile.GetFileFromPathAsync(path);
                var stream = await sf.OpenAsync(FileAccessMode.Read);
                if (onGetStream != null)
                {
                    onGetStream(sf, stream);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 把 LocalFolder 内的绝对路径转成 ms-appdata:///local/... 的 URI。
        ///
        /// 为什么要有这个：用 BitmapImage.SetSource(stream) 显示本地照片时，
        /// 每张图都要先 GetFileFromPathAsync + OpenAsync 走一遍 Storage broker，
        /// 再在 UI 线程上同步解码，而且 stream 方式**不参与 XAML 的图片缓存**，
        /// 每次进页面都要重新读盘、重新解码。主页一个相册卡片 7 张图，
        /// 五个相册就是 35 次这样的往返 —— 这就是「回主页要等一两秒才出图」的来源。
        ///
        /// 换成 UriSource 后，XAML 自己后台线程异步解码，并按 URI 缓存结果，
        /// 第二次进页面基本是直接命中缓存。
        ///
        /// 不在 LocalFolder 下的路径返回 null，调用方回退原来的 stream 方式。
        /// </summary>
        public static Uri TryGetLocalFolderUri(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            try
            {
                string root = ApplicationData.Current.LocalFolder.Path;
                if (string.IsNullOrEmpty(root)) return null;
                if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return null;

                string rel = path.Substring(root.Length).TrimStart('\\', '/');
                if (rel.Length == 0) return null;
                rel = rel.Replace('\\', '/');

                // 逐段 URL 编码：文件名里常有空格、中文、括号，不编码 Uri 构造会抛异常
                var sb = new StringBuilder("ms-appdata:///local/");
                string[] parts = rel.Split('/');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i > 0) sb.Append('/');
                    sb.Append(Uri.EscapeDataString(parts[i]));
                }

                return new Uri(sb.ToString());
            }
            catch
            {
                return null;
            }
        }

        public static async Task WriteLineAsync(string filename, string text)
        {
            var file = await KnownFolders.DocumentsLibrary.CreateFileAsync(
                filename, CreationCollisionOption.OpenIfExists);
            using (var ras = await file.OpenAsync(FileAccessMode.ReadWrite))
            using (var writer = new StreamWriter(ras.AsStreamForWrite()))
            {
                await writer.WriteLineAsync(text);
            }
        }

        public static async Task<string> ReadLineAsync(string filename)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                filename, CreationCollisionOption.OpenIfExists);

            // Win8.0 没有 OpenForReadAsync，用 OpenAsync
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            using (var reader = new StreamReader(stream.AsStreamForRead()))
            {
                return await reader.ReadLineAsync();
            }
        }
    }
}