using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.IO;

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

                using (var fs = store.OpenFile(
                    Consts.FILE_NAME, FileMode.Open))
                {
                    var reader = new DatabaseReader();
                    var root = reader.Load(fs, KeyCache.Password);

                    foreach (var group in root.Groups)
                    {
                        var item = new ListBoxItem();
                        item.Content = group.Name;

                        lstItems.Items.Add(item);
                    }
                }
            }
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            this.OpenSettings();
        }
    }
}