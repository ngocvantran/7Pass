using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
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
        private readonly ApplicationBarMenuItem _mnuReset;
        private readonly ApplicationBarIconButton _mnuSave;

        private EntryEx _binding;
        private Entry _entry;
        private bool _loaded;

        public EntryDetails()
        {
            InitializeComponent();

            _mnuSave = (ApplicationBarIconButton)
                ApplicationBar.Buttons[2];
            _mnuReset = (ApplicationBarMenuItem)
                ApplicationBar.MenuItems[0];
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            if (_loaded)
            {
                UpdateNotes();
                _binding.HasChanges = CurrentEntry.HasChanges;

                return;
            }

            var database = Cache.Database;
            if (database == null)
            {
                GoBack<MainPage>();
                return;
            }

            string id;
            var queries = NavigationContext.QueryString;

            Entry entry;
            if (queries.TryGetValue("id", out id))
            {
                entry = database.GetEntry(id);

                ThreadPool.QueueUserWorkItem(
                    _ => Cache.AddRecent(id));
            }
            else
                entry = new Entry();

            _loaded = true;
            DisplayEntry(entry);
        }

        private void DisplayEntry(Entry entry)
        {
            _entry = entry;

            _binding = new EntryEx(entry);
            _binding.HasChangesChanged += _binding_HasChangesChanged;
            _binding.HasChanges = entry.IsNew();

            CurrentEntry.Entry = entry;
            CurrentEntry.HasChanges = entry.IsNew();

            UpdateNotes();
            DataContext = _binding;

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
            return _entry.GetNavigateUrl(txtUrl.Text);
        }

        private void OpenUrl(bool useIntegreatedBrowser)
        {
            var url = GetUrl();
            if (string.IsNullOrEmpty(url))
                return;

            if (useIntegreatedBrowser)
            {
                this.NavigateTo<WebView>(
                    "url={0}&entry={1}", url, _entry.ID);

                return;
            }

            new WebBrowserTask
            {
                URL = url,
            }.Show();
        }

        private void Save()
        {
            SetWorkingState(true);

            var info = Cache.DbInfo;
            var writer = new DatabaseWriter();

            info.OpenDatabaseFile(x => writer
                .LoadExisting(x, info.Data.MasterKey));

            if (_entry.ID != null)
                writer.Details(_entry);
            else
            {
                var groupId = NavigationContext
                    .QueryString["group"];

                Cache.Database
                    .AddNew(_entry, groupId);

                writer.New(_entry);
            }

            info.SetDatabase(writer.Save);

            UpdateNotes();
            SetWorkingState(false);

            MessageBox.Show(
                Properties.Resources.SavedCaption,
                Properties.Resources.SavedTitle,
                MessageBoxButton.OK);

            ThreadPool.QueueUserWorkItem(
                _ => Cache.AddRecent(_entry.ID));
        }

        private void SetWorkingState(bool working)
        {
            IsEnabled = !working;
            progBusy.IsLoading = working;
            ApplicationBar.IsVisible = !working;
        }

        private void UpdateNotes()
        {
            var notes = _entry.Notes;

            if (!string.IsNullOrEmpty(notes))
            {
                notes = notes
                    .Replace(Environment.NewLine, " ")
                    .TrimStart();

                if (notes.Length > 60)
                {
                    notes = notes
                        .Substring(0, 55)
                        .TrimEnd() + "...";
                }
            }
            else
            {
                notes = Properties
                    .Resources.AddNotes;
            }

            lnkNotes.Content = notes;
        }

        private void _binding_HasChangesChanged(object sender, EventArgs e)
        {
            var hasChanges = _binding.HasChanges;

            _mnuSave.IsEnabled = hasChanges;
            _mnuReset.IsEnabled = hasChanges;
            CurrentEntry.HasChanges = hasChanges;
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

        private void cmdSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void lnkFields_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo<EntryFields>(
                "id={0}", _entry.ID);
        }

        private void lnkNotes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo<EntryNotes>(
                "id={0}", _entry.ID);
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

        private void mnuReset_Click(object sender, EventArgs e)
        {
            _binding.Reset();

            DataContext = null;
            DataContext = _binding;

            UpdateNotes();
        }

        private void txtUrl_Changed(object sender, TextChangedEventArgs e)
        {
            lnkUrl.Content = GetUrl();
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;

            if (txt != null)
                txt.SelectAll();
        }
    }
}