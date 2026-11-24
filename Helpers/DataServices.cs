using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MemoriesDemo.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MemoriesDemo.Helpers
{
    public partial class DataServices
    {
        private static readonly string MemoriesDir = "MemoriesDir";

        // ---------------- 同步兼容别名（供 App.cs / AlbumDetailsView.cs 调用） ----------------

        public static void SaveAlbumToStorage(AlbumVM album)
        {
            if (album == null) return;
            // fire-and-forget：内部异步保存
            SaveAlbumToStorageAsync(album);
        }

        // ---------------- 公共 API ----------------

        public static async Task<List<AlbumVM>> GetAlbumsFromStorageAsync()
        {
            var albums = new List<AlbumVM>();
            var folder = await EnsureAlbumStorageAsync();
            if (folder == null) return albums;

            var files = await folder.GetFilesAsync();
            foreach (var f in files)
            {
                var album = await GetAlbumFromStorageAsync(f);
                if (album != null) albums.Add(album);
            }
            return albums;
        }

        public static async Task<AlbumVM> GetAlbumFromStorageAsync(StorageFile file)
        {
            AlbumVM album = null;
            using (var ras = await file.OpenAsync(FileAccessMode.Read))
            using (var stream = ras.AsStreamForRead())
            using (var reader = new BinaryReader(stream))
            {
                album = new AlbumVM
                {
                    AlbumID    = new Guid(reader.ReadString()),
                    Title      = reader.ReadString(),
                    Caption    = reader.ReadString(),
                    CreatedBy  = reader.ReadString(),
                    DateAdded  = DateTime.Parse(reader.ReadString()),
                    Image      = reader.ReadString(),
                    ThemeName  = reader.ReadString(),
                    TemplateID = reader.ReadString()
                };

                int pageCount = reader.ReadInt32();
                for (int i = 0; i < pageCount; i++)
                {
                    var page = new PageVM
                    {
                        PageID   = reader.ReadString(),
                        PageVMID = new Guid(reader.ReadString())
                    };

                    int captionCount = reader.ReadInt32();
                    for (int j = 0; j < captionCount; j++)
                        page.Captions.Add(new TextPageItemVM { Caption = reader.ReadString() });

                    int mediaCount = reader.ReadInt32();
                    for (int k = 0; k < mediaCount; k++)
                    {
                        var media = new MediaItemVM
                        {
                            IsMediaFromStream = reader.ReadBoolean(),
                            MediaItemID       = new Guid(reader.ReadString()),
                            MediaSource       = reader.ReadString(),
                            PageInnerIndex    = reader.ReadInt32(),
                            CompositeTransform = new CompositeTransformData
                            {
                                TranslateX = reader.ReadDouble(),
                                TranslateY = reader.ReadDouble(),
                                ScaleX     = reader.ReadDouble(),
                                ScaleY     = reader.ReadDouble(),
                                SkewX      = reader.ReadDouble(),
                                SkewY      = reader.ReadDouble(),
                                Rotation   = reader.ReadDouble()
                            }
                        };

                        int propCount = reader.ReadInt32();
                        for (int l = 0; l < propCount; l++)
                        {
                            var prop = new PropData
                            {
                                ImageAsset = reader.ReadString(),
                                PanelCanvasHeight = reader.ReadDouble(),
                                PanelCanvasWidth  = reader.ReadDouble(),
                                CompositeTransform = new CompositeTransformData
                                {
                                    TranslateX = reader.ReadDouble(),
                                    TranslateY = reader.ReadDouble(),
                                    ScaleX     = reader.ReadDouble(),
                                    ScaleY     = reader.ReadDouble(),
                                    SkewX      = reader.ReadDouble(),
                                    SkewY      = reader.ReadDouble(),
                                    Rotation   = reader.ReadDouble()
                                }
                            };
                            media.Props.Add(prop);
                        }
                        page.MediaItems.Add(media);
                    }
                    album.Pages.Add(page);
                }
            }
            return album;
        }

        public static async Task SaveAlbumToStorageAsync(AlbumVM album)
        {
            if (album == null) return;
            if (album.IsDemoAlbum) return;

            var file = await CreateAlbumStorageFileAsync(album);
            if (file == null) return;

            using (var ras = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                ras.Size = 0;
                using (var stream = ras.AsStreamForWrite())
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(album.AlbumID.ToString());
                    writer.Write(album.Title ?? string.Empty);
                    writer.Write(album.Caption ?? string.Empty);
                    writer.Write(album.CreatedBy ?? string.Empty);
                    writer.Write(album.DateAdded.ToString());
                    writer.Write(album.Image ?? string.Empty);
                    writer.Write(album.ThemeName ?? string.Empty);
                    writer.Write(album.TemplateID ?? string.Empty);
                    writer.Write(album.Pages.Count);

                    foreach (PageVM page in album.Pages)
                    {
                        writer.Write(page.PageID ?? string.Empty);
                        writer.Write(page.PageVMID.ToString());

                        writer.Write(page.Captions.Count);
                        foreach (TextPageItemVM t in page.Captions)
                            writer.Write(t.Caption ?? string.Empty);

                        writer.Write(page.MediaItems.Count);
                        foreach (MediaItemVM m in page.MediaItems)
                        {
                            writer.Write(m.IsMediaFromStream);
                            writer.Write(m.MediaItemID.ToString());
                            writer.Write(m.MediaSource ?? string.Empty);
                            writer.Write(m.PageInnerIndex);
                            writer.Write(m.CompositeTransform.TranslateX);
                            writer.Write(m.CompositeTransform.TranslateY);
                            writer.Write(m.CompositeTransform.ScaleX);
                            writer.Write(m.CompositeTransform.ScaleY);
                            writer.Write(m.CompositeTransform.SkewX);
                            writer.Write(m.CompositeTransform.SkewY);
                            writer.Write(m.CompositeTransform.Rotation);

                            writer.Write(m.Props.Count);
                            foreach (PropData p in m.Props)
                            {
                                writer.Write(p.ImageAsset ?? string.Empty);
                                writer.Write(p.PanelCanvasHeight);
                                writer.Write(p.PanelCanvasWidth);
                                writer.Write(p.CompositeTransform.TranslateX);
                                writer.Write(p.CompositeTransform.TranslateY);
                                writer.Write(p.CompositeTransform.ScaleX);
                                writer.Write(p.CompositeTransform.ScaleY);
                                writer.Write(p.CompositeTransform.SkewX);
                                writer.Write(p.CompositeTransform.SkewY);
                                writer.Write(p.CompositeTransform.Rotation);
                            }
                        }
                    }
                }
            }
        }

        public static async Task DeleteAlbumFromStorageAsync(AlbumVM album)
        {
            if (album == null) return;
            var folder = await EnsureAlbumStorageAsync();
            if (folder == null) return;
            try
            {
                string name = string.Format("Album_{0}", album.AlbumID);
                var file = await folder.GetFileAsync(name);
                await file.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
            }
        }

        // ---------------- 私有辅助 ----------------

        private static async Task<StorageFolder> EnsureAlbumStorageAsync()
        {
            try
            {
                var folders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
                foreach (var f in folders)
                {
                    if (f.Path.Contains(MemoriesDir))
                        return f;
                }
                return await ApplicationData.Current.LocalFolder.CreateFolderAsync(MemoriesDir);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<StorageFile> CreateAlbumStorageFileAsync(AlbumVM album)
        {
            var folder = await EnsureAlbumStorageAsync();
            if (folder == null) return null;
            string name = string.Format("Album_{0}", album.AlbumID);
            return await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
        }
    }
}