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

        private EntryEx _binding;
        private Entry _entry;
        private bool _hasChanges;

        public EntryDetails()
        {
            InitializeComponent();

            _cmdSave = (ApplicationBarIconButton)
                ApplicationBar.Buttons[2];
            _cmdReset = (ApplicationBarIconButton)
                ApplicationBar.Buttons[3];
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

            var database = Cache.Database;
            if (database == null)
            {
                this.BackToDBs();
                return;
            }

            if (_binding != null)
            {
                _binding.Password = _entry.Password
                    ?? string.Empty;

                return;
            }

            var config = database.Configuration;
            txtTitle.IsProtected = config.ProtectTitle;
            txtPassword.IsProtected = config.ProtectPassword;
            txtUsername.IsProtected = config.ProtectUserName;

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
                    UserName = config.DefaultUserName,
                };

                txtTitle.Loaded += (sender, e1) =>
                    txtTitle.Focus();
            }

            DisplayEntry(entry);
        }

        private void CheckChangeState()
        {
            UpdateChangeState(_binding.DetectChanges());
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
            _binding.FieldChanged += _binding_Changed;
            _binding.PropertyChanged += _binding_Changed;

            CurrentEntry.Entry = entry;
            CurrentEntry.HasChanges = entry.IsNew();

            DataContext = _binding;
            UpdateChangeState(entry.IsNew());
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
                    "url={0}&entry={1}", url, _entry.ID);

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

                _binding.Save();
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
                    progBusy.IsBusy = false;
                    UpdateChangeState(false);

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

        private void UpdateChangeState(bool state)
        {
            if (state == _hasChanges)
                return;

            _hasChanges = state;
            _cmdSave.IsEnabled = _hasChanges;
            _cmdReset.IsEnabled = _hasChanges;
            CurrentEntry.HasChanges = _hasChanges;
        }

        private void _binding_Changed(object sender, EventArgs e)
        {
            CheckChangeState();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            if (ConfirmNavigateAway())
                this.BackToRoot();
        }

        private void cmdPassGen_Click(object sender, EventArgs e)
        {
            this.NavigateTo<PassGen>(
                "id={0}", _entry.ID);
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            Focus();

            Dispatcher.BeginInvoke(() =>
            {
                _binding.Reset();
                AnalyticsTracker.Track(
                    "modify", "reset_entry");

                DataContext = null;
                DataContext = _binding;
            });
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            Save();
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

        private void mnuIntegrated_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(true);
        }

        private void mnuRoot_Click(object sender, EventArgs e)
        {
            if (ConfirmNavigateAway())
                this.BackToDBs();
        }

        private void txtNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            var expression = txtNotes.GetBindingExpression(TextBox.TextProperty);
            if (expression != null)
                expression.UpdateSource();
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
            pivot.IsLocked = true;
            /* TODO: Problem: when the pivot is locked
             * and user tap on the tab headers area, an exception
             * is thrown
             * */

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

        private void txt_LostFocus(object sender, RoutedEventArgs e)
        {
            pivot.IsLocked = false;
        }
    }
}