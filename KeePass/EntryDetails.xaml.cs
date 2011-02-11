using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.IO;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class EntryDetails
    {
        private bool _loaded;

        public EntryDetails()
        {
            InitializeComponent();
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
            var lnkUrl = (HyperlinkButton) sender;
            this.NavigateTo<WebView>("url={0}&entry={1}",
                lnkUrl.Tag, NavigationContext.QueryString["id"]);
        }
    }
}