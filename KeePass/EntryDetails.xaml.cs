using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.IO.Data;
using KeePass.IO.Write;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace KeePass
{
    public partial class EntryDetails
    {
        private bool _loaded;
        private readonly ApplicationBarMenuItem _mnuSave;

        public EntryDetails()
        {
            InitializeComponent();

            _mnuSave = (ApplicationBarMenuItem)
                ApplicationBar.MenuItems[0];
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled || _loaded)
                return;

            var database = Cache.Database;
            if (database == null)
            {
                GoBack<MainPage>();
                return;
            }

            _loaded = true;
            var id = NavigationContext.QueryString["id"];

            DataContext = database.GetEntry(id);
            Cache.AddRecent(id);
        }

        private void OpenUrl(string url,
            bool useIntegreatedBrowser)
        {
            if (useIntegreatedBrowser)
            {
                this.NavigateTo<WebView>("url={0}&entry={1}",
                    url, NavigationContext.QueryString["id"]);
                return;
            }

            new WebBrowserTask
            {
                URL = url,
            }.Show();
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            GoBack<GroupDetails>();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            GoBack<MainPage>();
        }

        private void lnkUrl_Click(object sender, RoutedEventArgs e)
        {
            var lnkUrl = (HyperlinkButton)sender;
            var url = (string)lnkUrl.Tag;

            if (string.IsNullOrEmpty(url))
                return;

            var settings = AppSettings.Instance;
            OpenUrl(url, settings.UseIntBrowser);
        }

        private void mnuBrowser_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            OpenUrl((string)item.Tag, false);
        }

        private void mnuIntegrated_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            OpenUrl((string)item.Tag, true);
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;

            if (txt != null)
                txt.SelectAll();
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            var entry = (Entry)DataContext;
            entry.Password = txtPassword.Text;
            entry.UserName = txtUsername.Text;

            var info = Cache.DbInfo;
            DatabaseWriter.Save(info.Data, entry);
        }

        private void txt_Changed(object sender, TextChangedEventArgs e)
        {
            _mnuSave.IsEnabled = true;
        }
    }
}