using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Coding4Fun.Phone.Controls;
using KeePass.Analytics;
using KeePass.Controls;
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
        private readonly ApplicationBarIconButton _cmdReset;
        private readonly ApplicationBarIconButton _cmdSave;

        private EntryBinding _binding;
        private Entry _entry;

        public EntryDetails()
        {
            InitializeComponent();

            _cmdSave = AppButton(2);
            _cmdReset = AppButton(3);

            AppButton(0).Text = Langs.App.Home;
            AppButton(1).Text = Langs.EntryDetails.GeneratePassword;
            _cmdSave.Text = Langs.EntryDetails.Save;
            _cmdReset.Text = Langs.EntryDetails.Reset;

            AppMenuItem(1).Text = Langs.App.SelectDb;
            AppMenuItem(2).Text = Langs.App.About;
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

            if (_binding != null)
            {
                _binding.Save();

                UpdateNotes();
                UpdateFieldsCount(_entry);
                txtPassword.Text = _binding.Password;

                return;
            }

            var database = Cache.Database;
            if (database == null)
            {
                this.BackToDBs();
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
                var config = database.Configuration;

                entry = new Entry
                {
                    Password = Generator
                        .CharacterSets.NewEntry(),
                    UserName = config.DefaultUserName,
                    Protections =
                        {
                            Title = config.ProtectTitle,
                            UserName = config.ProtectUserName,
                            Password = config.ProtectPassword,
                        }
                };

                txtTitle.Loaded += (sender, e1) =>
                    txtTitle.Focus();
            }

            DisplayEntry(entry);
            UpdateFieldsCount(entry);
        }

        /// <summary>
        /// Checks if 7Pass can navigate away from this page.
        /// </summary>
        /// <returns></returns>
        private bool ConfirmNavigateAway()
        {
            if (!_binding.HasChanges)
                return true;

            var confirm = MessageBox.Show(
                Langs.EntryDetails.UnsavedChange,
                Langs.EntryDetails.UnsavedChangeTitle,
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

            var config = entry.Protections;
            txtTitle.IsProtected = config.Title;
            txtPassword.IsProtected = config.Password;
            txtUsername.IsProtected = config.UserName;

            _binding = new EntryBinding(entry);
            _binding.HasChangesChanged += _binding_HasChangesChanged;
            _binding.HasChanges = entry.IsNew();

            CurrentEntry.Entry = _binding;
            _binding.HasChanges = entry.IsNew();

            UpdateNotes();
            DataContext = _binding;
        }

        private void UpdateFieldsCount(Entry entry)
        {
            var mnuFields = AppMenuItem(0);
            mnuFields.Text = string.Format(
                Langs.EntryDetails.Fields,
                entry.CustomFields.Count);
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
                AnalyticsTracker.Track(
                    "browser", "integrated");

                this.NavigateTo<WebView>(
                    "url={0}", url);

                return;
            }

            AnalyticsTracker.Track(
                "browser", "external");

            new WebBrowserTask
            {
                Uri = new Uri(url),
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
                AnalyticsTracker.Track("modify",
                    _entry.ID != null
                        ? "save_entry"
                        : "new_entry");

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

                    if (!info.NotifyIfNotSyncable())
                    {
                        new ToastPrompt
                        {
                            Title = Langs.EntryDetails.SavedTitle,
                            Message = Langs.EntryDetails.SavedCaption,
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
            var notes = _binding.Notes;

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
                notes = Langs.EntryDetails.AddNotes;
            }

            lnkNotes.Content = notes;
        }

        private void _binding_HasChangesChanged(
            object sender, EventArgs e)
        {
            var hasChanges = _binding.HasChanges;

            _cmdSave.IsEnabled = hasChanges;
            _cmdReset.IsEnabled = hasChanges;
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            if (ConfirmNavigateAway())
                this.BackToRoot();
        }

        private void cmdPassGen_Click(object sender, EventArgs e)
        {
            this.NavigateTo<PassGen>();
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            _binding.Reset();
            AnalyticsTracker.Track(
                "modify", "reset_entry");

            DataContext = null;
            DataContext = _binding;

            UpdateNotes();
            UpdateFieldsCount(_entry);
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void lnkNotes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo<EntryNotes>();
        }

        private void lnkUrl_Click(object sender, RoutedEventArgs e)
        {
            var settings = AppSettings.Instance;
            OpenUrl(settings.UseIntBrowser);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void mnuBrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(false);
        }

        private void mnuFields_Click(object sender, EventArgs e)
        {
            this.NavigateTo<EntryFields>();
        }

        private void mnuIntegrated_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(true);
        }

        private void mnuRoot_Click(object sender, EventArgs e)
        {
            if (ConfirmNavigateAway())
                this.BackToDBs();
        }

        private void txtUrl_Changed(object sender, TextChangedEventArgs e)
        {
            var url = GetUrl();
            lnkUrl.Content = url;

            lnkUrl.IsEnabled = UrlUtils
                .IsValidUrl(url);
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;

            if (txt != null)
            {
                txt.SelectAll();
                return;
            }

            var protect = sender as ProtectedTextBox;
            if (protect != null)
                protect.SelectAll();
        }
    }
}