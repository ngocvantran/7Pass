using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.IO.Data;
using KeePass.IO.Write;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace KeePass
{
    public partial class EntryDetails
    {
        private readonly ApplicationBarMenuItem _mnuSave;

        private bool _loaded;

        public EntryDetails()
        {
            InitializeComponent();

            _mnuSave = (ApplicationBarMenuItem)
                ApplicationBar.MenuItems[0];
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            if (_loaded)
            {
                if (EntryState.HasChanges)
                {
                    var entry = (Entry)DataContext;

                    UpdateNotes(entry);
                    _mnuSave.IsEnabled = true;
                }

                return;
            }

            EntryState.HasChanges = false;

            var database = Cache.Database;
            if (database == null)
            {
                GoBack<MainPage>();
                return;
            }

            _loaded = true;

            var id = NavigationContext.QueryString["id"];
            DisplayEntry(database.GetEntry(id));

            ThreadPool.QueueUserWorkItem(
                _ => Cache.AddRecent(id));
        }

        private void DisplayEntry(Entry entry)
        {
            UpdateNotes(entry);
            DataContext = entry;
            lnkUrl.Content = GetUrl();

            var fields = entry.CustomFields.Count;
            if (fields == 0)
            {
                lnkFields.Visibility =
                    Visibility.Collapsed;
            }
            else
            {
                lnkFields.Visibility =
                    Visibility.Visible;

                lnkFields.Content = string.Format(
                    Properties.Resources.FieldsLink,
                    fields);
            }
        }

        private string GetUrl()
        {
            var entry = (Entry)DataContext;
            return entry.GetNavigateUrl(txtUrl.Text);
        }

        private void OpenUrl(bool useIntegreatedBrowser)
        {
            var url = GetUrl();
            if (string.IsNullOrEmpty(url))
                return;

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

        private void UpdateNotes(Entry entry)
        {
            var notes = entry.Notes;

            notes = notes
                .Replace(Environment.NewLine, " ")
                .TrimStart();

            if (notes.Length > 60)
            {
                notes = notes
                    .Substring(0, 55)
                    .TrimEnd() + "...";
            }

            lnkNotes.Content = notes;
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

        private void lnkFields_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo<EntryFields>("id={0}",
                NavigationContext.QueryString["id"]);
        }

        private void lnkNotes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo<EntryNotes>("id={0}",
                NavigationContext.QueryString["id"]);
        }

        private void lnkUrl_Click(object sender, RoutedEventArgs e)
        {
            var settings = AppSettings.Instance;
            OpenUrl(settings.UseIntBrowser);
        }

        private void mnuBrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(false);
        }

        private void mnuIntegrated_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(true);
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            var entry = (Entry)DataContext;

            entry.Url = txtUrl.Text;
            entry.Title = txtTitle.Text;
            entry.Password = txtPassword.Text;
            entry.UserName = txtUsername.Text;

            var info = Cache.DbInfo;
            var writer = new DatabaseWriter();

            info.OpenDatabaseFile(x => writer
                .LoadExisting(x, info.Data.MasterKey));

            writer.Details(entry);
            info.SetDatabase(writer.Save);
        }

        private void txtUrl_Changed(object sender, TextChangedEventArgs e)
        {
            txt_Changed(sender, e);
            lnkUrl.Content = GetUrl();
        }

        private void txt_Changed(object sender, TextChangedEventArgs e)
        {
            _mnuSave.IsEnabled = true;
            EntryState.HasChanges = true;
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;

            if (txt != null)
                txt.SelectAll();
        }
    }
}