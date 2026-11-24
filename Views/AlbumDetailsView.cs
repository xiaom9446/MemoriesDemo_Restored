using System;
using System.Collections.Generic;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace MemoriesDemo.Views
{
    public partial class AlbumDetailsView : Page
    {
        // ===== 业务字段（界面控件由 XAML 生成，不在此重复声明）=====
        private AlbumVM _albumInstance;
        private CoreDispatcher _cd;
        private MediaPageItemView _currentMediaPageItemView;

        // ===== AppBar 状态 =====
        private enum AppBarMenu
        {
            None,
            Main,       // 浏览：Edit
            Editing,    // 编辑：Add Page / Reorder Pages / Done
            MediaAdd,   // 空格：Capture / Add
            MediaEdit,  // 已有媒体：Add Adorner / Crop / Change / Remove
            Reorder     // 重排：Delete Selected / Done
        }

        private bool _isAppBarOpen;
        private bool _isReorderMode;
        private bool _editModeSyncPending;
        private DateTime _lastEditSyncTime = DateTime.MinValue;
        private AppBarMenu _appBarMenu = AppBarMenu.None;

        public AlbumDetailsView()
        {
            this.InitializeComponent();

            this._cd = Window.Current.Dispatcher;

            // ===== 事件绑定 =====
            if (this._btn_EditMode != null)       this._btn_EditMode.Click       += OnBtn_EditMode_Click;
            if (this._btn_Done != null)           this._btn_Done.Click           += OnBtn_EditMode_Click;
            if (this._btn_AddPage != null)        this._btn_AddPage.Click        += OnBtn_AddPage_Click;
            if (this._btn_ReorderPages != null)   this._btn_ReorderPages.Click   += OnBtn_ReorderPages_Click;
            if (this._btn_ReorderDone != null)    this._btn_ReorderDone.Click    += OnBtn_ReorderDone_Click;
            if (this._btn_DeleteSelected != null) this._btn_DeleteSelected.Click += OnBtn_DeleteSelected_Click;

            if (this._btn_AddAdorner != null)    this._btn_AddAdorner.Click    += OnMediaPageItemView_Btn_Edit_Click;
            if (this._btn_Delete != null)        this._btn_Delete.Click        += OnMediaPageItemView_Btn_Delete_Click;
            if (this._btn_Capture_Photo != null) this._btn_Capture_Photo.Click += OnMediaPageItemView_Btn_Capture_Click;
            if (this._btn_Add != null)           this._btn_Add.Click           += OnMediaPageItemView_Btn_Add_Click;
            if (this._btn_Change != null)        this._btn_Change.Click        += OnMediaPageItemView_Btn_Change_Click;
            if (this._btn_CropMedia != null)     this._btn_CropMedia.Click     += OnBtn_CropMedia_Click;

            if (this._pageView_Main != null)
            {
                this._pageView_Main.ItemTemplateSelector = new AlbumPageTemplateSelector();
                this._pageView_Main.SelectionChanged += OnPageView_SelectionChanged;

                // 虚拟化：滚动后新建的页面视图拿不到编辑态。
                // ContainerContentChanging 是 Win8.1 才有的，8.0 只能用 LayoutUpdated 兜底；
                // 布局每帧都会触发，所以里面按 300ms 节流，避免编辑态下反复遍历可视化树。
                this._pageView_Main.LayoutUpdated += this.OnPageViewLayoutUpdated;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (MainPage._s_Instance != null)
                MainPage._s_Instance.NavigationInProgress = true;

            this._albumInstance = MainViewModel.Instance.CurrentAlbum;
            base.DataContext = this._albumInstance;
            MainViewModel.Instance.CurrentView = View.AlbumDetailsView;

            if (this._gridPlaceholder != null)
                this._gridPlaceholder.MinWidth = Window.Current.Bounds.Width;

            this._isReorderMode = false;

            if (this._albumInstance != null)
            {
                this.ResetMediaMenu();
                this.UpdateEditMode(this._albumInstance.IsEditing);

                if (this._albumInstance.IsNewAlbum)
                {
                    this._albumInstance.IsNewAlbum = false;
                    this.AddNewPage();
                }
                else if (this._albumInstance.IsGenerated)
                {
                    this._albumInstance.IsGenerated = false;
                    this.LaunchMediaPicker();
                }
                else
                {
                    if (this._pageView_Main != null)
                        this._pageView_Main.ItemsSource = this._albumInstance.Pages;

                    foreach (PageVM page in this._albumInstance.Pages)
                    {
                        page.IsEditing = this._albumInstance.IsEditing;
                        page.ShowAppBarMediaMenuItems += OnPage_ShowAppBarMediaMenuItems;
                    }
                }

                // 编辑态进入时直接弹出 AppBar（与原始应用一致）
                if (this._albumInstance.IsEditing)
                    this.SetAppBar(true, true);
            }

            this.UpdatePageCount();

            if (this.sb_TransitionIn != null)
            {
                this.sb_TransitionIn.Completed += (s, ev) =>
                {
                    if (MainPage._s_Instance != null)
                        MainPage._s_Instance.NavigationInProgress = false;
                };
                this.sb_TransitionIn.Begin();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.SetAppBar(false, false);

            if (this._albumInstance != null)
            {
                if (this._albumInstance.IsEditing)
                    this.SaveAlbum();

                this._albumInstance.IsEditing = false;
                foreach (PageVM page in this._albumInstance.Pages)
                {
                    page.TriggerUnload = true;
                    page.ShowAppBarMediaMenuItems -= OnPage_ShowAppBarMediaMenuItems;
                }
            }
            if (this.sb_TransitionOut != null)
                this.sb_TransitionOut.Begin();
        }

        // ================== AppBar ==================

        private void OnLayoutRoot_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // 右键切换 AppBar 的显示/隐藏
            if (this._isAppBarOpen)
            {
                this.SetAppBar(false, false);
                e.Handled = true;
                return;
            }

            this.SetAppBar(true, this._albumInstance != null && this._albumInstance.IsEditing);
            e.Handled = true;
        }

        private void SetAppBar(bool isOpen, bool isPersistent)
        {
            if (this._appbar == null) return;

            this._isAppBarOpen = isOpen;

            if (isOpen)
            {
                this._appbar.Visibility = Visibility.Visible;
                this.RefreshAppBarMenu();
                if (this.sb_AppBarIn != null) this.sb_AppBarIn.Begin();
            }
            else
            {
                if (this.sb_AppBarOut != null && this._appbar.Visibility == Visibility.Visible)
                    this.sb_AppBarOut.Begin();

                this._appbar.Visibility = Visibility.Collapsed;
                this._appbar.Opacity = 1.0;
                this._appBarMenu = AppBarMenu.None;
                this.ShowAppBarPanel(AppBarMenu.None);
            }
        }

        /// <summary>根据当前状态决定 AppBar 上该显示哪一组按钮。</summary>
        private void RefreshAppBarMenu()
        {
            if (this._isReorderMode)
            {
                this.ShowAppBarPanel(AppBarMenu.Reorder);
                return;
            }

            if (this._currentMediaPageItemView != null && this._currentMediaPageItemView.MediaItemVM != null)
            {
                bool hasMedia = !string.IsNullOrEmpty(this._currentMediaPageItemView.MediaItemVM.MediaSource);
                this.ShowAppBarPanel(hasMedia ? AppBarMenu.MediaEdit : AppBarMenu.MediaAdd);
                return;
            }

            if (this._albumInstance != null && this._albumInstance.IsEditing)
            {
                this.ShowAppBarPanel(AppBarMenu.Editing);
                return;
            }

            this.ShowAppBarPanel(AppBarMenu.Main);
        }

        private void ShowAppBarPanel(AppBarMenu menu)
        {
            this._appBarMenu = menu;

            bool isEditing = this._albumInstance != null && this._albumInstance.IsEditing;
            bool mediaMenu = menu == AppBarMenu.MediaAdd || menu == AppBarMenu.MediaEdit;

            // 媒体按钮出现在页面按钮的左边，两者可以同时显示（与原应用一致）
            if (this._appbar_MediaAdd != null)
                this._appbar_MediaAdd.Visibility = menu == AppBarMenu.MediaAdd ? Visibility.Visible : Visibility.Collapsed;
            if (this._appbar_MediaEdit != null)
                this._appbar_MediaEdit.Visibility = menu == AppBarMenu.MediaEdit ? Visibility.Visible : Visibility.Collapsed;

            if (this._btn_EditMode != null)
                this._btn_EditMode.Visibility = (menu == AppBarMenu.Main || (menu == AppBarMenu.None && !isEditing))
                    ? Visibility.Visible : Visibility.Collapsed;

            if (this._appbar_Pages != null)
            {
                bool showPages = isEditing && menu != AppBarMenu.Reorder;
                this._appbar_Pages.Visibility = showPages ? Visibility.Visible : Visibility.Collapsed;
            }

            if (this._appbar_Reorder != null)
                this._appbar_Reorder.Visibility = menu == AppBarMenu.Reorder ? Visibility.Visible : Visibility.Collapsed;

            if (this._tb_AppBarHint != null)
            {
                if (menu == AppBarMenu.Reorder) this._tb_AppBarHint.Text = "重排页面";
                else if (mediaMenu) this._tb_AppBarHint.Text = "媒体";
                else if (isEditing) this._tb_AppBarHint.Text = "编辑相册";
                else this._tb_AppBarHint.Text = "Memories";
            }
        }

        private void UpdatePageCount()
        {
            if (this._tb_PageCount == null || this._albumInstance == null) return;

            int pageCount = this._albumInstance.Pages == null ? 0 : this._albumInstance.Pages.Count;
            int mediaCount = this._albumInstance.AllMediaItems == null ? 0 : this._albumInstance.AllMediaItems.Count;

            this._tb_PageCount.Text = string.Format("{0} 页 · {1} 项媒体", pageCount, mediaCount);
        }

        // ---------- 媒体菜单（现在挂在 AppBar 上） ----------
        private void ResetMediaMenu()
        {
            if (this._currentMediaPageItemView != null)
            {
                this._currentMediaPageItemView.UpdateSelection(false);
                this._currentMediaPageItemView = null;
            }

            if (this._isAppBarOpen)
                this.RefreshAppBarMenu();
        }

        private void OnPageView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this._isReorderMode || this._btn_DeleteSelected == null) return;

            this._btn_DeleteSelected.IsEnabled =
                this._pageView_Main != null && this._pageView_Main.SelectedItem is PageVM;
        }

        private void OnPage_ShowAppBarMediaMenuItems(object sender, MenuItemEventArgs e)
        {
            if (e.MediaPageItemView == null || e.MediaPageItemView.MediaItemVM == null) return;
            if (this._albumInstance == null || !this._albumInstance.IsEditing) return;
            if (this._isReorderMode) return;

            if (this._currentMediaPageItemView != null && this._currentMediaPageItemView != e.MediaPageItemView)
                this._currentMediaPageItemView.UpdateSelection(false);

            this._currentMediaPageItemView = e.MediaPageItemView;
            this._currentMediaPageItemView.UpdateSelection(true);

            this.SetAppBar(true, false);
        }

        private void OnBtn_EditMode_Click(object sender, RoutedEventArgs e)
        {
            this._albumInstance.IsEditing = !this._albumInstance.IsEditing;
            this.UpdateEditMode(this._albumInstance.IsEditing);
            this.ResetMediaMenu();

            this.SetAppBar(true, this._albumInstance.IsEditing);

            if (!this._albumInstance.IsEditing)
            {
                this.SaveAlbum();
                this.HideReorderPageView();
            }

            foreach (PageVM page in this._albumInstance.Pages)
                page.IsEditing = this._albumInstance.IsEditing;

            // PageVM.IsEditing 只发通知、不会自己流到已经创建好的页面视图里，
            // 所以这里直接对已生成的页面视图补一次 BeginEdit / CompleteEdit，
            // 否则点 Edit 之后页面上的文字和图片仍然是不可编辑的。
            this.ApplyEditMode(this._albumInstance.IsEditing);
        }

        /// <summary>把编辑态下发到当前已经实例化的页面视图。</summary>
        private void ApplyEditMode(bool isEditing)
        {
            if (this._pageView_Main == null) return;

            // WinRT 的 GridView（ItemsControl）没有 ContainerFromItem 这个方法，
            // 只有 ItemContainerGenerator 上有，而且虚拟化未生成的项会返回 null。
            // 这里直接遍历可视化树，把已经实例化的页面视图统一刷一遍，最稳。
            var views = new List<IGenericAlbumPageView>();
            CollectPageViews(this._pageView_Main, views);

            for (int i = 0; i < views.Count; i++)
            {
                // 单个页面视图内部数据不完整时不要连坐其它页
                try
                {
                    if (isEditing) AlbumPageViewHelper.BeginEdit(views[i]);
                    else AlbumPageViewHelper.CompleteEdit(views[i]);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 布局变化后补一次编辑态同步：GridView 虚拟化时新生成的页面视图
        /// 拿不到之前那次 BeginEdit，只能靠这里补。
        /// 300ms 节流 + pending 合并，避免滚动时每帧都遍历一遍可视化树。
        /// </summary>
        private void OnPageViewLayoutUpdated(object sender, object e)
        {
            if (this._albumInstance == null || !this._albumInstance.IsEditing) return;
            if (this._pageView_Main == null) return;
            if (this._editModeSyncPending) return;

            var now = DateTime.Now;
            if ((now - this._lastEditSyncTime).TotalMilliseconds < 300.0) return;
            this._lastEditSyncTime = now;

            this._editModeSyncPending = true;

            this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                this._editModeSyncPending = false;

                // 期间可能已经退出编辑态
                if (this._albumInstance == null || !this._albumInstance.IsEditing) return;
                this.ApplyEditMode(true);
            });
        }

        /// <summary>递归收集页面视图；命中后不再深入，避免把内部重复元素算进来。</summary>
        private static void CollectPageViews(DependencyObject root, List<IGenericAlbumPageView> result)
        {
            if (root == null) return;

            var view = root as IGenericAlbumPageView;
            if (view != null)
            {
                result.Add(view);
                return;
            }

            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                CollectPageViews(VisualTreeHelper.GetChild(root, i), result);
            }
        }

        private void UpdateEditMode(bool isEditing)
        {
            // 标题：文本 / 编辑框 二选一
            if (this._tb_Title != null)
                this._tb_Title.Visibility = isEditing ? Visibility.Collapsed : Visibility.Visible;
            if (this._tb_TitleEdit != null)
                this._tb_TitleEdit.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;

            if (this._isAppBarOpen)
                this.RefreshAppBarMenu();
        }

        private void OnBtn_AddPage_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewPage();
        }

        private void AddNewPage()
        {
            var picker = new AlbumPageTemplatePicker();
            picker.TransitionOutComplete += OnPageTemplatePickerComplete;
            DialogHelper.ShowDialog(picker);
        }

        private void OnPageTemplatePickerComplete(object sender, object e)
        {
            var picker = sender as AlbumPageTemplatePicker;
            if (picker != null)
                picker.TransitionOutComplete -= OnPageTemplatePickerComplete;

            if (picker != null && picker.DialogResult != null)
            {
                PageVM pageToAdd = picker.DialogResult;
                pageToAdd.ShowAppBarMediaMenuItems += OnPage_ShowAppBarMediaMenuItems;
                pageToAdd.IsEditing = this._albumInstance != null && this._albumInstance.IsEditing;

                if (this._pageView_Main != null)
                    this._pageView_Main.ItemsSource = null;

                this._albumInstance.Pages.Add(pageToAdd);

                if (this._pageView_Main != null)
                    this._pageView_Main.ItemsSource = this._albumInstance.Pages;

                this.UpdatePageCount();
                this.SetAppBar(true, true);

                var capturedGv = this._pageView_Main;
                if (capturedGv != null)
                {
                    System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                    {
                        var ignore = capturedGv.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () => capturedGv.ScrollIntoView(pageToAdd));
                    });
                }
            }
        }

        private void OnBtn_DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (this._pageView_Main == null || this._albumInstance == null) return;

            var page = this._pageView_Main.SelectedItem as PageVM;
            if (page == null) return;

            page.ShowAppBarMediaMenuItems -= OnPage_ShowAppBarMediaMenuItems;
            this._albumInstance.Pages.Remove(page);

            this._pageView_Main.SelectedIndex = -1;
            if (this._btn_DeleteSelected != null) this._btn_DeleteSelected.IsEnabled = false;

            this.UpdatePageCount();
            this.SetAppBar(true, true);
        }

        private void OnBtn_ReorderPages_Click(object sender, RoutedEventArgs e)
        {
            this.ShowReorderPageView();
        }

        private void OnBtn_ReorderDone_Click(object sender, RoutedEventArgs e)
        {
            this.HideReorderPageView();
            this.SetAppBar(true, true);
        }

        /// <summary>
        /// 进入重排：不再切换第二个 GridView（那里一旦模板/状态出问题就是整片空白），
        /// 而是复用当前这个已经在正常显示页面的 GridView ——
        /// 只把容器改成 320x320 的缩略图尺寸并打开单选，页面模板内部自带 Viewbox，会自动等比缩小。
        /// </summary>
        private void ShowReorderPageView()
        {
            if (this._albumInstance == null || this._pageView_Main == null) return;

            this._isReorderMode = true;
            this.ResetMediaMenu();

            try
            {
                foreach (PageVM page in this._albumInstance.Pages)
                {
                    if (page == null || page.MediaItems == null) continue;
                    foreach (MediaItemVM item in page.MediaItems)
                        if (item != null) item.IsReadOnly = true;
                }
            }
            catch
            {
            }

            var gv = this._pageView_Main;

            var thumbStyle = this.Resources["ReorderItemContainerStyle"] as Style;
            if (thumbStyle != null) gv.ItemContainerStyle = thumbStyle;

            gv.Visibility = Visibility.Visible;
            gv.Opacity = 1.0;
            gv.IsItemClickEnabled = true;
            gv.SelectionMode = ListViewSelectionMode.Single;

            // 换容器样式后必须让容器重新生成，否则还是旧尺寸
            gv.ItemsSource = null;
            gv.ItemsSource = this._albumInstance.Pages;
            gv.SelectedIndex = -1;

            if (this._btn_DeleteSelected != null)
                this._btn_DeleteSelected.IsEnabled = false;

            this.SetAppBar(true, true);
        }

        /// <summary>退出重排：容器样式、选择模式、只读标记全部还原。</summary>
        private void HideReorderPageView()
        {
            if (!this._isReorderMode) return;

            this._isReorderMode = false;

            try
            {
                foreach (PageVM page in this._albumInstance.Pages)
                {
                    if (page == null || page.MediaItems == null) continue;
                    foreach (MediaItemVM item in page.MediaItems)
                        if (item != null) item.IsReadOnly = false;
                }
            }
            catch
            {
            }

            var gv = this._pageView_Main;
            if (gv != null)
            {
                var pageStyle = this.Resources["PageItemContainerStyle"] as Style;
                if (pageStyle != null) gv.ItemContainerStyle = pageStyle;

                gv.SelectionMode = ListViewSelectionMode.None;
                gv.IsItemClickEnabled = false;

                gv.ItemsSource = null;
                gv.ItemsSource = this._albumInstance == null ? null : this._albumInstance.Pages;
                gv.SelectedIndex = -1;

                gv.Visibility = Visibility.Visible;
                gv.Opacity = 1.0;
            }

            if (this._btn_DeleteSelected != null)
                this._btn_DeleteSelected.IsEnabled = false;

            if (this._isAppBarOpen)
                this.RefreshAppBarMenu();
        }

        private void OnMediaPageItemView_Btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentMediaPageItemView != null)
                this._currentMediaPageItemView.OnEditMediaItem();
        }

        private void OnMediaPageItemView_Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentMediaPageItemView != null)
            {
                this._currentMediaPageItemView.OnDeleteMediaItem();
                this.UpdatePageCount();

                // 移除后变成空媒体格，AppBar 切到 Capture / Add
                if (this._isAppBarOpen) this.RefreshAppBarMenu();
            }
        }

        private void OnMediaPageItemView_Btn_Change_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentMediaPageItemView == null) return;

            this._currentMediaPageItemView.PickFile(result =>
            {
                if (result)
                {
                    this.UpdatePageCount();
                    // 换完图后从“Capture/Add”切到“Adorner/Crop/Change/Remove”
                    if (this._isAppBarOpen) this.RefreshAppBarMenu();
                    else this.SetAppBar(true, true);
                }
            });
        }

        private void OnMediaPageItemView_Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentMediaPageItemView != null)
            {
                this._currentMediaPageItemView.PickFile(result =>
                {
                    if (result)
                    {
                        this.UpdatePageCount();
                        if (this._isAppBarOpen) this.RefreshAppBarMenu();
                    }
                });
            }
        }

        private void OnMediaPageItemView_Btn_Capture_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentMediaPageItemView != null)
            {
                this._currentMediaPageItemView.CapturePhoto(result =>
                {
                    if (result)
                    {
                        this.UpdatePageCount();
                        if (this._isAppBarOpen) this.RefreshAppBarMenu();
                    }
                });
            }
        }

        private void OnBtn_CropMedia_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentMediaPageItemView != null && MainViewModel.Instance.CurrentPage != null)
            {
                var editPageView = new EditPageView();
                editPageView.TransitionOutComplete += OnEditPageDialogComplete;
                DialogHelper.ShowDialog(editPageView);
            }
        }

        private void OnEditPageDialogComplete(object sender, object e)
        {
            var nav = sender as INavigationElement;
            if (nav != null)
                nav.TransitionOutComplete -= OnEditPageDialogComplete;

            MainViewModel.Instance.CurrentView = View.AlbumDetailsView;
            if (this._pageView_Main != null)
            {
                this._pageView_Main.ItemsSource = null;
                this._pageView_Main.ItemsSource = this._albumInstance.Pages;
            }
            this.ResetMediaMenu();
            this.UpdatePageCount();
        }

        private void SaveAlbum()
        {
            if (MainViewModel.Instance.IsNewAlbum)
                MainViewModel.Instance.IsNewAlbum = false;

            if (string.IsNullOrEmpty(this._albumInstance.Image))
            {
                this._albumInstance.RefreshMediaList();
                if (this._albumInstance.AllMediaItems.Count > 0)
                    this._albumInstance.Image = ((MediaItemVM)this._albumInstance.AllMediaItems[0]).MediaSource;
            }

            if (!this._albumInstance.IsDemoAlbum)
                DataServices.SaveAlbumToStorage(this._albumInstance);
        }

        /// <summary>
        /// Generate Album：选择图片后生成页面。
        /// 关键点：选中的文件大多在“图片库”之外（桌面、下载目录等），
        /// Win8 应用无法用路径再次读取，必须复制到应用本地文件夹后再引用。
        /// </summary>
        private async void LaunchMediaPicker()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

            if (files == null || files.Count == 0)
            {
                this.SetAppBar(true, true);
                return;
            }

            // 先复制到本地文件夹，得到 App 之后一直可读的路径
            var localPaths = new List<string>();
            foreach (StorageFile file in files)
            {
                try
                {
                    StorageFile copy = await file.CopyAsync(
                        ApplicationData.Current.LocalFolder,
                        file.Name,
                        NameCollisionOption.GenerateUniqueName);
                    localPaths.Add(copy.Path);
                }
                catch
                {
                }
            }

            await this._cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this._albumInstance.Pages.Clear();

                int i = 0;
                while (i < localPaths.Count)
                {
                    int pageCount;
                    var page = new PageVM();
                    page.PageID = Helper.GetRandomPageID(out pageCount);

                    for (int k = 0; k < pageCount && i < localPaths.Count; k++, i++)
                    {
                        var item = new MediaItemVM(localPaths[i]);
                        item.PageInnerIndex = k;
                        item.IsMediaFromStream = true;
                        page.MediaItems.Add(item);
                    }

                    page.IsEditing = this._albumInstance.IsEditing;
                    page.ShowAppBarMediaMenuItems += OnPage_ShowAppBarMediaMenuItems;
                    this._albumInstance.Pages.Add(page);
                }

                if (this._pageView_Main != null)
                    this._pageView_Main.ItemsSource = this._albumInstance.Pages;

                this.UpdatePageCount();
                this.SetAppBar(true, true);
            });
        }
    }
}
