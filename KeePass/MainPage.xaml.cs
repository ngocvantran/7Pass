using System;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;

namespace KeePass
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (!store.FileExists(Consts.FILE_NAME))
                {
                    this.OpenDownload();
                    return;
                }

                if (!KeyCache.HasPassword)
                {
                    this.NavigateTo("/Password.xaml");
                    return;
                }
            }
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            this.OpenSettings();
        }
    }
}