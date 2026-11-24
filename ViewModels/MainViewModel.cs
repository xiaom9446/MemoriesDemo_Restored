// MainViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using MemoriesDemo.Helpers;

namespace MemoriesDemo.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel _instance;
        private ObservableList _albums;
        private AlbumVM _currentAlbum;
        private AlbumVM _tempCurrentAlbumCopy;
        private bool _isNewAlbum;
        private MediaItemVM _currentMediaItemVM;
        private List<object> _themes;
        private List<object> _mediaAdorners;
        private List<object> _propAssets;
        private PageVM _currentPage;

        public static MainViewModel Instance
        {
            get
            {
                if (MainViewModel._instance == null)
                {
                    MainViewModel._instance = new MainViewModel();
                }
                return MainViewModel._instance;
            }
        }

        private MainViewModel()
        {
            this.InitTempFakeData();

            this._themes = new List<object>
            {
                "Summer Vacation",
                "Wedding",
                "Birthday",
                "Party"
            };

            this._mediaAdorners = new List<object>
            {
                "add props",
                "add scenes"
            };

            this.PageTemplates = new List<object>
            {
                "PageTemplate_Scrap_1",
                "PageTemplate_Scrap_2",
                "PageTemplate_Scrap_3",
                "PageTemplate_Scrap_4",
                "PageTemplate_Scrap_5",
                "PageTemplate_Scrap_6",
                "PageTemplate_Scrap_7",
                "PageTemplate_Scrap_8",
                "PageTemplate_Basic_1",
                "PageTemplate_Basic_2",
                "PageTemplate_Basic_3",
                "PageTemplate_Basic_4",
                "PageTemplate_Basic_5",
                "PageTemplate_Basic_6",
                "PageTemplate_Basic_7"
            };

            this._propAssets = new List<object>
            {
                "Assets\\Prop_SunTan.png",
                "Assets\\Prop_Flippers.png",
                "Assets\\Prop_IceCream.png",
                "Assets\\Prop_Crab.png",
                "Assets\\Prop_Sun.png",
                "Assets\\Prop_Ball.png",
                "Assets\\Prop_BathingSuit.png",
                "Assets\\Prop_Bottle.png",
                "Assets\\Prop_Bucket.png",
                "Assets\\Prop_Glasses.png",
                "Assets\\Prop_Radio.png",
                "Assets\\Prop_Shell.png",
                "Assets\\Prop_Shorts.png",
                "Assets\\Prop_Shovel.png",
                "Assets\\Prop_StarFish.png",
                "Assets\\Prop_BathingSuit.png",
                "Assets\\Prop_Bottle.png",
                "Assets\\Prop_Bucket.png",
                "Assets\\Prop_Glasses.png",
                "Assets\\Prop_Radio.png",
                "Assets\\Prop_Shell.png",
                "Assets\\Prop_Shorts.png",
                "Assets\\Prop_Shovel.png",
                "Assets\\Prop_StarFish.png"
            };
        }

        public ObservableList Albums
        {
            get { return this._albums; }
            set { this._albums = value; this.OnPropertyChanged("Albums"); }
        }

        public AlbumVM CurrentAlbum
        {
            get { return this._currentAlbum; }
            set { this._currentAlbum = value; this.OnPropertyChanged("CurrentAlbum"); }
        }

        public AlbumVM TempCurrentAlbumCopy
        {
            get { return this._tempCurrentAlbumCopy; }
            set { this._tempCurrentAlbumCopy = value; this.OnPropertyChanged("TempCurrentAlbumCopy"); }
        }

        public List<object> Themes
        {
            get { return this._themes; }
            set { this._themes = value; this.OnPropertyChanged("Themes"); }
        }

        public List<object> MediaAdorners
        {
            get { return this._mediaAdorners; }
            set { this._mediaAdorners = value; this.OnPropertyChanged("MediaAdorners"); }
        }

        public List<object> PropAssets
        {
            get { return this._propAssets; }
            set { this._propAssets = value; this.OnPropertyChanged("PropAssets"); }
        }

        public List<object> PageTemplates { get; set; }

        public bool IsNewAlbum
        {
            get { return this._isNewAlbum; }
            set { this._isNewAlbum = value; this.OnPropertyChanged("IsNewAlbum"); }
        }

        public MediaItemVM CurrentMediaItemVM
        {
            get { return this._currentMediaItemVM; }
            set { this._currentMediaItemVM = value; this.OnPropertyChanged("CurrentMediaItemVM"); }
        }

        public PageVM CurrentPage
        {
            get { return this._currentPage; }
            set { this._currentPage = value; this.OnPropertyChanged("CurrentPage"); }
        }

        public View CurrentView { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitTempFakeData()
        {
            List<object> list = new List<object>();
            list.Add(this.CreateFakeDataItem6("AlbumItemTemplateView", "Our Daughter Growing Up"));
            list.Add(this.CreateFakeDataItem5("AlbumItemTemplateView2", "Surfs Up"));
            list.Add(this.CreateFakeDataItem2("AlbumItemTemplateView2", "My Daughter's Birthday Party"));
            list.Add(this.CreateFakeDataItem4("AlbumItemTemplateView", "Skate Park"));
            list.Add(this.CreateFakeDataItem3("AlbumItemTemplateView3", "Our Pets"));
            this._albums = new ObservableList(list);
        }

        private AlbumVM CreateFakeDataItem1(string albumTemplateID, string albumTitle)
        {
            AlbumVM albumVM = new AlbumVM();
            albumVM.AlbumID = Guid.NewGuid();
            albumVM.Caption = "Antarctica";
            albumVM.DateAdded = DateTime.UtcNow;
            albumVM.Image = "Assets\\Antarctica\\antarctica1 (1024x768).jpg";
            albumVM.ThemeName = "";
            albumVM.Title = albumTitle;
            albumVM.TemplateID = albumTemplateID;
            albumVM.IsDemoAlbum = true;
            albumVM.MediaItems = new List<MediaItemVM>();

            List<object> pages = new List<object>();

            PageVM page1 = new PageVM();
            page1.PageID = "PageTemplate_Scrap_1";
            page1.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Antarctica\\antarctica1 (1024x768).jpg" }
            };
            page1.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Beautiful ." },
                new TextPageItemVM { Caption = "In hac habitasse platea dictumst. Maecenas urna eros, ultricies accumsan interdum eget, auctor sodales eros. Nam imperdiet ut adipiscing diam viverra." }
            };
            pages.Add(page1);

            PageVM page2 = new PageVM();
            page2.PageID = "PageTemplate_Scrap_2";
            page2.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Antarctica\\antarctica2 (1024x768).jpg" }
            };
            page2.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Etiam sed auctor sapien." },
                new TextPageItemVM { Caption = "Sed risus risus, ultricies quis sollicitudin quis, convallis ac dolor. Mauris bibendum molestie orci eget posuere. Fusce tincidunt accumsan tincidunt." }
            };
            pages.Add(page2);

            PageVM page3 = new PageVM();
            page3.PageID = "PageTemplate_Scrap_3";
            page3.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Antarctica\\antarctica3 (1024x768).jpg" }
            };
            page3.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Praesent varius, velit quis euismod condimentum, turpis." }
            };
            pages.Add(page3);

            PageVM page4 = new PageVM();
            page4.PageID = "PageTemplate_Scrap_6";
            page4.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Antarctica\\antarctica5 (1024x768).jpg" }
            };
            page4.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Aenean accumsan lobortis augue in facilisis. Nunc dignissim metus condimentum ante egestas et euismod felis elementum." }
            };
            pages.Add(page4);

            albumVM.Pages = new ObservableList(pages);
            return albumVM;
        }

        private AlbumVM CreateFakeDataItem2(string albumTemplateID, string albumTitle)
        {
            AlbumVM albumVM = new AlbumVM();
            albumVM.AlbumID = Guid.NewGuid();
            albumVM.Caption = "My Daughter's Birthday";
            albumVM.DateAdded = DateTime.UtcNow;
            albumVM.Image = "Assets\\Birthday Party\\WIN11_-11578.jpg";
            albumVM.ThemeName = "";
            albumVM.Title = albumTitle;
            albumVM.TemplateID = albumTemplateID;
            albumVM.IsDemoAlbum = true;
            albumVM.MediaItems = new List<MediaItemVM>();

            List<object> pages = new List<object>();

            PageVM p1 = new PageVM();
            p1.PageID = "PageTemplate_Scrap_1";
            p1.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-11578.jpg" } };
            p1.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Happy Birthday to my girl!" }, new TextPageItemVM { Caption = "My little princess is posing" } };
            pages.Add(p1);

            PageVM p2 = new PageVM();
            p2.PageID = "PageTemplate_Scrap_2";
            p2.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-11698.jpg" } };
            p2.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "That smile!" }, new TextPageItemVM { Caption = "Opening her first present of the day, she is so excited to just rip it open. " } };
            pages.Add(p2);

            PageVM p3 = new PageVM();
            p3.PageID = "PageTemplate_Scrap_3";
            p3.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-11787.jpg" } };
            p3.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Trying to capture a moment before she grows older and flies away" } };
            pages.Add(p3);

            PageVM p4 = new PageVM();
            p4.PageID = "PageTemplate_Scrap_4";
            p4.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12030.jpg" } };
            p4.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Best friends enjoying the day together" } };
            pages.Add(p4);

            PageVM p5 = new PageVM();
            p5.PageID = "PageTemplate_Scrap_5";
            p5.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12036.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12045.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12051.jpg" }
            };
            p5.Captions = new List<TextPageItemVM>();
            pages.Add(p5);

            PageVM p6 = new PageVM();
            p6.PageID = "PageTemplate_Scrap_6";
            p6.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12193.jpg" } };
            p6.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "That crown and cape is too cute." } };
            pages.Add(p6);

            PageVM p7 = new PageVM();
            p7.PageID = "PageTemplate_Scrap_7";
            p7.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12215.jpg" } };
            p7.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "One of my favorites of the day. He loved that toy" } };
            pages.Add(p7);

            PageVM p8 = new PageVM();
            p8.PageID = "PageTemplate_Scrap_5";
            p8.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12472.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\Birthday Party.wmv" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12481.jpg" }
            };
            p8.Captions = new List<TextPageItemVM>();
            pages.Add(p8);

            PageVM p9 = new PageVM();
            p9.PageID = "PageTemplate_Scrap_6";
            p9.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12510.jpg" } };
            p9.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "A magical day with enjoyment had by all." } };
            pages.Add(p9);

            PageVM p10 = new PageVM();
            p10.PageID = "PageTemplate_Scrap_7";
            p10.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12516.jpg" } };
            p10.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "My girl was a little afraid of getting dirty from the bubbles." } };
            pages.Add(p10);

            PageVM p11 = new PageVM();
            p11.PageID = "PageTemplate_Scrap_4";
            p11.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Birthday Party\\WIN11_-12520.jpg" } };
            p11.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "By the end of the day, all the kids were extremely tired and needing a nap" } };
            pages.Add(p11);

            albumVM.Pages = new ObservableList(pages);
            return albumVM;
        }

        private AlbumVM CreateFakeDataItem3(string albumTemplateID, string albumTitle)
        {
            AlbumVM albumVM = new AlbumVM();
            albumVM.AlbumID = Guid.NewGuid();
            albumVM.Caption = "Our Pets";
            albumVM.DateAdded = DateTime.UtcNow;
            albumVM.Image = "Assets\\Our Pets\\Bailey 1.jpg";
            albumVM.ThemeName = "";
            albumVM.Title = albumTitle;
            albumVM.TemplateID = albumTemplateID;
            albumVM.IsDemoAlbum = true;
            albumVM.MediaItems = new List<MediaItemVM>();

            List<object> pages = new List<object>();

            PageVM p1 = new PageVM();
            p1.PageID = "PageTemplate_Basic_1";
            p1.MediaItems = new List<MediaItemVM>();
            p1.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Our beautiful pets" },
                new TextPageItemVM { Caption = "Our pets are part of our family. They complete us. They make us whole. They bring us up when we are sad and share in all of our joyous memories. " },
                new TextPageItemVM { Caption = "Here is my tribute to the wonders of our lives." }
            };
            pages.Add(p1);

            PageVM p2 = new PageVM();
            p2.PageID = "PageTemplate_Basic_2";
            p2.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\Bailey 1.jpg" } };
            p2.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Our Protector." } };
            pages.Add(p2);

            PageVM p3 = new PageVM();
            p3.PageID = "PageTemplate_Basic_3";
            p3.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\Bailey 2.jpg" } };
            p3.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "I love those eyes." },
                new TextPageItemVM { Caption = "Champ is not resting. He is carefully watching over us." }
            };
            pages.Add(p3);

            PageVM p4 = new PageVM();
            p4.PageID = "PageTemplate_Basic_5";
            p4.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\IMG_0250.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\IMG_0652.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\IMG_0830.jpg" },
                new MediaItemVM { PageInnerIndex = 3, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\IMG_1469.jpg" }
            };
            p4.Captions = new List<TextPageItemVM>();
            pages.Add(p4);

            PageVM p5 = new PageVM();
            p5.PageID = "PageTemplate_Basic_4";
            p5.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\Spot 1.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\Spot 2.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\Spot 3.jpg" },
                new MediaItemVM { PageInnerIndex = 3, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\Spot 3.jpg" }
            };
            p5.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "My family." },
                new TextPageItemVM { Caption = "The youngest member." }
            };
            pages.Add(p5);

            PageVM p6 = new PageVM();
            p6.PageID = "PageTemplate_Basic_2";
            p6.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Our Pets\\Spot 1.jpg" } };
            p6.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Our newest member." } };
            pages.Add(p6);

            albumVM.Pages = new ObservableList(pages);
            return albumVM;
        }

        private AlbumVM CreateFakeDataItem4(string albumTemplateID, string albumTitle)
        {
            AlbumVM albumVM = new AlbumVM();
            albumVM.AlbumID = Guid.NewGuid();
            albumVM.Caption = "Skate Park";
            albumVM.DateAdded = DateTime.UtcNow;
            albumVM.Image = "Assets\\Skate Park\\WIN11_-24728.jpg";
            albumVM.ThemeName = "";
            albumVM.Title = albumTitle;
            albumVM.TemplateID = albumTemplateID;
            albumVM.IsDemoAlbum = true;
            albumVM.MediaItems = new List<MediaItemVM>();

            List<object> pages = new List<object>();

            PageVM p1 = new PageVM();
            p1.PageID = "PageTemplate_Scrap_1";
            p1.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-24728.jpg" } };
            p1.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "A day at the park" },
                new TextPageItemVM { Caption = "This was one of the best days of the summer. Nothing to do but catch air!" }
            };
            pages.Add(p1);

            PageVM p2 = new PageVM();
            p2.PageID = "PageTemplate_Scrap_2";
            p2.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-24957.jpg" } };
            p2.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "First trick of the day." },
                new TextPageItemVM { Caption = "Here we go!" }
            };
            pages.Add(p2);

            PageVM p3 = new PageVM();
            p3.PageID = "PageTemplate_Basic_5";
            p3.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25051.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25383.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25427.jpg" },
                new MediaItemVM { PageInnerIndex = 3, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25520.jpg" }
            };
            p3.Captions = new List<TextPageItemVM>();
            pages.Add(p3);

            PageVM p4 = new PageVM();
            p4.PageID = "PageTemplate_Scrap_3";
            p4.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25527.jpg" } };
            p4.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Such a professional" } };
            pages.Add(p4);

            PageVM p5 = new PageVM();
            p5.PageID = "PageTemplate_Basic_4";
            p5.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25539.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\Skating 2.wmv" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25966.jpg" },
                new MediaItemVM { PageInnerIndex = 3, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25966.jpg" }
            };
            p5.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Watch the video" },
                new TextPageItemVM { Caption = "Oh man!" }
            };
            pages.Add(p5);

            PageVM p6 = new PageVM();
            p6.PageID = "PageTemplate_Scrap_3";
            p6.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-25797.jpg" } };
            p6.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Taking a much needed break." } };
            pages.Add(p6);

            PageVM p7 = new PageVM();
            p7.PageID = "PageTemplate_Scrap_6";
            p7.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-26012.jpg" } };
            p7.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Up and around, and around, and around..." } };
            pages.Add(p7);

            PageVM p8 = new PageVM();
            p8.PageID = "PageTemplate_Scrap_7";
            p8.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Skate Park\\WIN11_-26194.jpg" } };
            p8.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "On our way out, we always have time for a smile" } };
            pages.Add(p8);

            albumVM.Pages = new ObservableList(pages);
            return albumVM;
        }

        private AlbumVM CreateFakeDataItem5(string albumTemplateID, string albumTitle)
        {
            AlbumVM albumVM = new AlbumVM();
            albumVM.AlbumID = Guid.NewGuid();
            albumVM.Caption = "Surf's Up";
            albumVM.DateAdded = DateTime.UtcNow;
            albumVM.Image = "Assets\\Surfs Up\\WIN11_-15580.jpg";
            albumVM.ThemeName = "";
            albumVM.Title = albumTitle;
            albumVM.TemplateID = albumTemplateID;
            albumVM.IsDemoAlbum = true;
            albumVM.MediaItems = new List<MediaItemVM>();

            List<object> pages = new List<object>();

            PageVM p1 = new PageVM();
            p1.PageID = "PageTemplate_Scrap_2";
            p1.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-15580.jpg" } };
            p1.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "An amazing day by the ocean." },
                new TextPageItemVM { Caption = "Waking up at the crack of dawn to embark on the calm of the ocean." }
            };
            pages.Add(p1);

            PageVM p2 = new PageVM();
            p2.PageID = "PageTemplate_Scrap_3";
            p2.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-15607.jpg" } };
            p2.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Walking towards greatness" },
                new TextPageItemVM { Caption = "Getting mentally prepared to talking the waves." }
            };
            pages.Add(p2);

            PageVM p3 = new PageVM();
            p3.PageID = "PageTemplate_Scrap_4";
            p3.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-15669.jpg" } };
            p3.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Such an amazing day" },
                new TextPageItemVM { Caption = "This was a great moment" }
            };
            pages.Add(p3);

            PageVM p4 = new PageVM();
            p4.PageID = "PageTemplate_Basic_5";
            p4.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\Surfing.wmv" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-15737.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-15893.jpg" },
                new MediaItemVM { PageInnerIndex = 3, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-15924.jpg" }
            };
            p4.Captions = new List<TextPageItemVM>();
            pages.Add(p4);

            PageVM p5 = new PageVM();
            p5.PageID = "PageTemplate_Scrap_1";
            p5.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-15960.jpg" } };
            p5.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "On our way out" },
                new TextPageItemVM { Caption = "Gameplan in our heads. Now to the water" }
            };
            pages.Add(p5);

            PageVM p6 = new PageVM();
            p6.PageID = "PageTemplate_Scrap_5";
            p6.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16269.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16287.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16287.jpg" }
            };
            p6.Captions = new List<TextPageItemVM>();
            pages.Add(p6);

            PageVM p7 = new PageVM();
            p7.PageID = "PageTemplate_Scrap_3";
            p7.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16294.jpg" } };
            p7.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "I remember exactly how I was feeling at this moment" } };
            pages.Add(p7);

            PageVM p8 = new PageVM();
            p8.PageID = "PageTemplate_Scrap_6";
            p8.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16437.jpg" } };
            p8.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "A successful day by the ocean." } };
            pages.Add(p8);

            PageVM p9 = new PageVM();
            p9.PageID = "PageTemplate_Scrap_7";
            p9.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16522.jpg" } };
            p9.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Everyone had a blast." } };
            pages.Add(p9);

            PageVM p10 = new PageVM();
            p10.PageID = "PageTemplate_Scrap_5";
            p10.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16740.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16745.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16758.jpg" }
            };
            p10.Captions = new List<TextPageItemVM>();
            pages.Add(p10);

            PageVM p11 = new PageVM();
            p11.PageID = "PageTemplate_Basic_5";
            p11.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16781.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16784.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16791.jpg" },
                new MediaItemVM { PageInnerIndex = 3, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16797.jpg" }
            };
            p11.Captions = new List<TextPageItemVM>();
            pages.Add(p11);

            PageVM p12 = new PageVM();
            p12.PageID = "PageTemplate_Scrap_6";
            p12.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16805.jpg" } };
            p12.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "This was a tough one to catch" } };
            pages.Add(p12);

            PageVM p13 = new PageVM();
            p13.PageID = "PageTemplate_Scrap_7";
            p13.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16806.jpg" } };
            p13.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Finally getting a good angle on the wave" } };
            pages.Add(p13);

            PageVM p14 = new PageVM();
            p14.PageID = "PageTemplate_Basic_4";
            p14.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16814.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16817.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16818.jpg" },
                new MediaItemVM { PageInnerIndex = 3, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-16818.jpg" }
            };
            p14.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Look at those skills!" } };
            pages.Add(p14);

            PageVM p15 = new PageVM();
            p15.PageID = "PageTemplate_Scrap_5";
            p15.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-19411.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-19510.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-19571.jpg" }
            };
            p15.Captions = new List<TextPageItemVM>();
            pages.Add(p15);

            PageVM p16 = new PageVM();
            p16.PageID = "PageTemplate_Scrap_2";
            p16.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-27148.jpg" } };
            p16.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Coming back in" },
                new TextPageItemVM { Caption = "After a long day of surfing, we really were tan" }
            };
            pages.Add(p16);

            PageVM p17 = new PageVM();
            p17.PageID = "PageTemplate_Scrap_3";
            p17.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-27289.jpg" } };
            p17.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Looking to attack the wave" },
                new TextPageItemVM { Caption = "Maximum air!" }
            };
            pages.Add(p17);

            PageVM p18 = new PageVM();
            p18.PageID = "PageTemplate_Scrap_4";
            p18.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-27354.jpg" } };
            p18.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Surf is up" },
                new TextPageItemVM { Caption = "" }
            };
            pages.Add(p18);

            PageVM p19 = new PageVM();
            p19.PageID = "PageTemplate_Scrap_1";
            p19.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Surfs Up\\WIN11_-27436.jpg" } };
            p19.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Amazing" },
                new TextPageItemVM { Caption = "One of the best days of surfing I've had in a long time." }
            };
            pages.Add(p19);

            albumVM.Pages = new ObservableList(pages);
            return albumVM;
        }

        private AlbumVM CreateFakeDataItem6(string albumTemplateID, string albumTitle)
        {
            AlbumVM albumVM = new AlbumVM();
            albumVM.AlbumID = Guid.NewGuid();
            albumVM.Caption = "Daughter Growing Up";
            albumVM.DateAdded = DateTime.UtcNow;
            albumVM.Image = "Assets\\Daughter Growing Up\\AnandH-12 (860x591).jpg";
            albumVM.ThemeName = "";
            albumVM.Title = albumTitle;
            albumVM.TemplateID = albumTemplateID;
            albumVM.IsDemoAlbum = true;
            albumVM.MediaItems = new List<MediaItemVM>();

            List<object> pages = new List<object>();

            PageVM p1 = new PageVM();
            p1.PageID = "PageTemplate_Scrap_1";
            p1.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-12 (860x591).jpg" } };
            p1.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "It was only yesterday..." },
                new TextPageItemVM { Caption = "When we carried her home from the hospital. I remember driving so slowly on the way back thinking of my precious cargo." }
            };
            pages.Add(p1);

            PageVM p2 = new PageVM();
            p2.PageID = "PageTemplate_Scrap_2";
            p2.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-19 (591x860).jpg" } };
            p2.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "First Swing" },
                new TextPageItemVM { Caption = "Her smile melts my heart in a way that is utterly indescribable. I would do anything for my pride and joy." }
            };
            pages.Add(p2);

            PageVM p3 = new PageVM();
            p3.PageID = "PageTemplate_Scrap_3";
            p3.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-20 (593x860).jpg" } };
            p3.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Picking flowers by the park. She did a great job!" } };
            pages.Add(p3);

            PageVM p4 = new PageVM();
            p4.PageID = "PageTemplate_Scrap_6";
            p4.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-21 (593x860).jpg" } };
            p4.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Running to show Mommy her day's work of effort" } };
            pages.Add(p4);

            PageVM p5 = new PageVM();
            p5.PageID = "PageTemplate_Scrap_5";
            p5.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-22 (860x591).jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-23 (591x860).jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-22 (860x591).jpg" }
            };
            p5.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Such an inquisitive mind already at a young age. Nothing gets by her." } };
            pages.Add(p5);

            PageVM p6 = new PageVM();
            p6.PageID = "PageTemplate_Scrap_7";
            p6.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-24 (591x860).jpg" } };
            p6.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Such an inquisitive mind already at a young age. Nothing gets by her." } };
            pages.Add(p6);

            PageVM p7 = new PageVM();
            p7.PageID = "PageTemplate_Scrap_1";
            p7.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-25 (591x860).jpg" } };
            p7.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Playing Fetch" },
                new TextPageItemVM { Caption = "I was so proud of her today. She was not scared at all and enjoyed herself throughout the afternoon." }
            };
            pages.Add(p7);

            PageVM p8 = new PageVM();
            p8.PageID = "PageTemplate_Scrap_2";
            p8.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-28 (860x591).jpg" } };
            p8.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "My little girl" },
                new TextPageItemVM { Caption = "That smile melts my heart everytime. " }
            };
            pages.Add(p8);

            PageVM p9 = new PageVM();
            p9.PageID = "PageTemplate_Scrap_3";
            p9.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\Daughter Growing Up\\AnandH-29 (860x591).jpg" } };
            p9.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "She is growing up so fast. Glad I will always have this memory." } };
            pages.Add(p9);

            albumVM.Pages = new ObservableList(pages);
            return albumVM;
        }

        private AlbumVM CreateFakeDataItem(string albumTemplateID, string albumTitle)
        {
            AlbumVM albumVM = new AlbumVM();
            albumVM.AlbumID = Guid.NewGuid();
            albumVM.Caption = "Our wedding on November 22nd";
            albumVM.DateAdded = DateTime.UtcNow;
            albumVM.Image = "Assets\\example_WeddingPic2.jpg";
            albumVM.ThemeName = "Wedding";
            albumVM.Title = albumTitle;
            albumVM.TemplateID = albumTemplateID;
            albumVM.IsDemoAlbum = true;

            albumVM.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM
                {
                    MediaItemID = Guid.NewGuid(),
                    MediaSource = "Assets\\example_WeddingPic1.jpg",
                    Caption = "... the five best days or as one of my Pommy work mates would say - my five besties."
                }
            };

            List<object> pages = new List<object>();

            PageVM p1 = new PageVM();
            p1.PageID = "PageTemplate_Scrap_1";
            p1.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic7.jpg" } };
            p1.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Everyone on the dance floor." },
                new TextPageItemVM { Caption = "Spending quality time with friends and family. Everyone on the dance floor celebrating." }
            };
            pages.Add(p1);

            PageVM p2 = new PageVM();
            p2.PageID = "PageTemplate_Scrap_2";
            p2.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic1.jpg" } };
            p2.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "Beautiful flowers!" },
                new TextPageItemVM { Caption = "November 22nd... A day to remember." }
            };
            pages.Add(p2);

            PageVM p3 = new PageVM();
            p3.PageID = "PageTemplate_Scrap_3";
            p3.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic6.jpg" } };
            p3.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "You guys are amazing!" } };
            pages.Add(p3);

            PageVM p4 = new PageVM();
            p4.PageID = "PageTemplate_Scrap_4";
            p4.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic9.jpg" } };
            p4.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "It's my first wedding!" } };
            pages.Add(p4);

            PageVM p5 = new PageVM();
            p5.PageID = "PageTemplate_Scrap_5";
            p5.MediaItems = new List<MediaItemVM>
            {
                new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic10.jpg" },
                new MediaItemVM { PageInnerIndex = 1, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic2.jpg" },
                new MediaItemVM { PageInnerIndex = 2, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic8.jpg" }
            };
            p5.Captions = new List<TextPageItemVM>();
            pages.Add(p5);

            PageVM p6 = new PageVM();
            p6.PageID = "PageTemplate_Scrap_6";
            p6.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic1.jpg" } };
            p6.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "... the five best days or as one of my Pommy work mates would say - my five besties." } };
            pages.Add(p6);

            PageVM p7 = new PageVM();
            p7.PageID = "PageTemplate_Scrap_7";
            p7.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic2.jpg", Caption = "enjoying the music." } };
            p7.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "our day to remember..." } };
            pages.Add(p7);

            PageVM p8 = new PageVM();
            p8.PageID = "PageTemplate_Scrap_8";
            p8.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic3.jpg" } };
            p8.Captions = new List<TextPageItemVM>();
            pages.Add(p8);

            PageVM p9 = new PageVM();
            p9.PageID = "PageTemplate_Scrap_2";
            p9.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic8.jpg" } };
            p9.Captions = new List<TextPageItemVM>
            {
                new TextPageItemVM { Caption = "It's a toast!" },
                new TextPageItemVM { Caption = "Let's toast to this day." }
            };
            pages.Add(p9);

            PageVM p10 = new PageVM();
            p10.PageID = "PageTemplate_Scrap_4";
            p10.MediaItems = new List<MediaItemVM> { new MediaItemVM { PageInnerIndex = 0, MediaItemID = Guid.NewGuid(), MediaSource = "Assets\\example_WeddingPic10.jpg" } };
            p10.Captions = new List<TextPageItemVM> { new TextPageItemVM { Caption = "Getting down on the dance floor.  Nice moves." } };
            pages.Add(p10);

            albumVM.Pages = new ObservableList(pages);
            return albumVM;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}