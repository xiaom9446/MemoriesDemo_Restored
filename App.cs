using System;
using MemoriesDemo.Helpers;
using MemoriesDemo.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MemoriesDemo
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
            // Win8.0 没有 Application.Exiting，删掉
            Application.Current.Resuming += OnApplication_Resuming;
            Application.Current.Suspending += OnApplication_Suspending;
        }

        private void OnApplication_Resuming(object sender, object e)
        {
        }

        private async void OnApplication_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                if (MainViewModel.Instance.CurrentAlbum != null)
                {
                    await DataServices.SaveAlbumToStorageAsync(MainViewModel.Instance.CurrentAlbum);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.Exception;
            if (exception is System.Runtime.InteropServices.COMException)
            {
                string msg = string.Concat(new object[]
                {
                    "Unhandled COMException Error caught: \nHResult: ",
                    ((System.Runtime.InteropServices.COMException)exception).HResult,
                    "\nMessage: ",
                    e.Message
                });
            }
            else
            {
                string msg = string.Concat(new object[]
                {
                    "Unhandled Error caught: \nException type: ",
                    exception.GetType(),
                    "\nMessage: ",
                    e.Message
                });
            }
            e.Handled = true;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Window.Current.Content = new MainPage();
            Window.Current.Activate();
        }

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            base.OnSearchActivated(args);
            if (Window.Current.Content == null)
            {
                MainPage mainPage = new MainPage();
                Window.Current.Content = mainPage;
                mainPage.NavigationInProgress = false;
            }
            (Window.Current.Content as MainPage).NavigateToSearchPage(args.QueryText);
            Window.Current.Activate();
        }
    }
}