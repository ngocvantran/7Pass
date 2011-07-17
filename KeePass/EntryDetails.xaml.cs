using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Coding4Fun.Phone.Controls;
using KeePass.Analytics;
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
        private readonly ApplicationBarIconButton _cmdSave;
        private readonly ApplicationBarMenuItem _mnuReset;

        private EntryEx _binding;
        private Entry _entry;

        public EntryDetails()
        {
            InitializeComponent();

            _cmdSave = (ApplicationBarIconButton)
                ApplicationBar.Buttons[2];
            _mnuReset = (ApplicationBarMenuItem)
                ApplicationBar.MenuItems[0];
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (!e.Cancel && !ConfirmNavigateAway())
                e.Cancel = true;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            if (_entry != null)
            {
                UpdateNotes();
                txtPassword.Text = _entry.Password
                    ?? string.Empty;

                _binding.HasChanges =
                    CurrentEntry.HasChanges;

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
            {
                entry = new Entry
                {
                    Password = Generator
                        .CharacterSets.NewEntry(),
                };

                txtTitle.Loaded += (sender, e1) =>
                    txtTitle.Focus();
            }

            DisplayEntry(entry);
        }

        /// <summary>
        /// Checks if 7Pass can navigate away from this page.
        /// </summary>
        /// <returns></returns>
        private bool ConfirmNavigateAway()
        {
            if (!CurrentEntry.HasChanges)
                return true;

            var confirm = MessageBox.Show(
                Properties.Resources.UnsavedChange,
                Properties.Resources.UnsavedChangeTitle,
                MessageBoxButton.OKCancel);

            if (confirm != MessageBoxResult.OK)
                return false;

            if (!_entry.IsNew())
            {
                DataContext = null;
                _entry.Reset();
            }

            return true;
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
                AnalyticsTracker.Track("int_browser");

                this.NavigateTo<WebView>(
                    "url={0}&entry={1}", url, _entry.ID);

                return;
            }

            AnalyticsTracker.Track("open_url");

            new WebBrowserTask
            {
                URL = url,
            }.Show();
        }

        private void Save()
        {
            progBusy.IsBusy = true;

            string groupId;
            if (!NavigationContext.QueryString
                .TryGetValue("group", out groupId))
            {
                groupId = null;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                AnalyticsTracker.Track(_entry.ID != null
                    ? "save_entry" : "new_entry");

                var info = Cache.DbInfo;
                var database = Cache.Database;
                var writer = new DatabaseWriter();

                info.OpenDatabaseFile(x => writer
                    .LoadExisting(x, info.Data.MasterKey));

                if (_entry.ID != null)
                    writer.Details(_entry);
                else
                {
                    database.AddNew(
                        _entry, groupId);

                    writer.New(_entry);
                }

                info.SetDatabase(x => writer.Save(
                    x, database.RecycleBin));

                Dispatcher.BeginInvoke(() =>
                {
                    UpdateNotes();
                    progBusy.IsBusy = false;

                    _binding.HasChanges = false;
                    CurrentEntry.HasChanges = false;

                    if (!info.NotifyIfNotSyncable())
                    {
                        new ToastPrompt
                        {
                            Title = Properties.Resources.SavedTitle,
                            Message = Properties.Resources.SavedCaption,
                            TextOrientation = System.Windows.Controls
                                .Orientation.Vertical,
                        }.Show();
                    }
                });

                ThreadPool.QueueUserWorkItem(
                    __ => Cache.AddRecent(_entry.ID));
            });
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

            _cmdSave.IsEnabled = hasChanges;
            _mnuReset.IsEnabled = hasChanges;
            CurrentEntry.HasChanges = hasChanges;
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            if (ConfirmNavigateAway())
                GoBack<GroupDetails>();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            if (ConfirmNavigateAway())
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

        private void mnuPassGen_Click(object sender, EventArgs e)
        {
            this.NavigateTo<PassGen>(
                "id={0}", _entry.ID);
        }

        private void mnuReset_Click(object sender, EventArgs e)
        {
            _binding.Reset();
            AnalyticsTracker.Track("reset_entry");

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