using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.Sources;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class MainPage
    {
        private readonly ObservableCollection<DatabaseItem> _items;
        private readonly ApplicationBarMenuItem _mnuUpdateAll;

        private bool _moved;

        public MainPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            _items = new ObservableCollection<DatabaseItem>();
            lstDatabases.ItemsSource = _items;

            _mnuUpdateAll = (ApplicationBarMenuItem)
                ApplicationBar.MenuItems[0];
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            _moved = false;

            if (cancelled)
            {
                _moved = true;
                return;
            }

            SourceCapabilityUpdater.Update();

            if (AppSettings.Instance.AllowAnalytics == null)
            {
                _moved = true;
                this.NavigateTo<AnalyticsSettings>();
                return;
            }

            Cache.Clear();

            var checkTileOpen = e.NavigationMode !=
                NavigationMode.Back;
            RefreshDbList(checkTileOpen);
        }

        private void DatabaseUpdated(DatabaseInfo info,
            SyncResults result, string error)
        {
            var listItem = _items.FirstOrDefault(
                x => x.Info == info);
            if (listItem == null)
                return;
            
            var dispatcher = Dispatcher;
            dispatcher.BeginInvoke(() =>
                listItem.IsUpdating = false);

            switch (result)
            {
                case SyncResults.NoChange:
                case SyncResults.Downloaded:
                    dispatcher.BeginInvoke(() =>
                        UpdateItem(listItem, "updated"));
                    break;

                case SyncResults.Uploaded:

                    dispatcher.BeginInvoke(() =>
                        UpdateItem(listItem, "uploaded"));
                    break;

                case SyncResults.Conflict:
                    dispatcher.BeginInvoke(() =>
                    {
                        UpdateItem(listItem, "uploaded");

                        MessageBox.Show(error,
                            Properties.Resources.ConflictTitle,
                            MessageBoxButton.OK);
                    });
                    break;

                case SyncResults.Failed:
                    var msg = string.Format(
                        Properties.Resources.UpdateFailure,
                        info.Details.Name, error);

                    dispatcher.BeginInvoke(() =>
                    {
                        listItem.UpdatedIcon = null;

                        MessageBox.Show(msg,
                            Properties.Resources.UpdateTitle,
                            MessageBoxButton.OK);
                    });
                    break;
            }
        }

        private void ListDatabases(string tile)
        {
            var dispatcher = Dispatcher;
            var databases = DatabaseInfo.GetAll();

            var open = tile == null ? null
                : databases.FirstOrDefault(
                    x => x.Folder == tile);

            if (open != null)
            {
                dispatcher.BeginInvoke(
                    () => Open(open, true));

                return;
            }

            foreach (var db in databases)
                db.LoadDetails();

            var items = databases
                .Where(x => x.Details != null)
                .Select(x => new DatabaseItem(x))
                .OrderBy(x => x.Name)
                .ToList();

            foreach (var item in items)
            {
                var local = item;
                dispatcher.BeginInvoke(() =>
                {
                    UpdateItem(local, null);
                    _items.Add(local);
                });
            }

            var hasUpdatables = items
                .Any(x => x.CanUpdate);

            dispatcher.BeginInvoke(() =>
                _mnuUpdateAll.IsEnabled = hasUpdatables);
        }

        private void Open(DatabaseInfo database,
            bool fromTile)
        {
            if (!fromTile)
            {
                if (!database.HasPassword)
                {
                    this.NavigateTo<Password>(
                        "db={0}", database.Folder);
                }
                else
                {
                    database.Open(Dispatcher);
                    this.NavigateTo<GroupDetails>();
                }
            }
            else
            {
                if (!database.HasPassword)
                {
                    this.NavigateTo<Password>(
                        "db={0}&fromTile=1", database.Folder);
                }
                else
                {
                    database.Open(Dispatcher);
                    this.NavigateTo<GroupDetails>("fromTile=1");
                }
            }
        }

        private void RefreshDbList(bool checkTileOpen)
        {
            _items.Clear();

            string tile;
            if (!checkTileOpen || !NavigationContext
                .QueryString.TryGetValue("tile", out tile))
            {
                tile = null;
            }

            ThreadPool.QueueUserWorkItem(
                _ => ListDatabases(tile));
        }

        private void Update(DatabaseItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (!Network.CheckNetwork())
                return;

            item.IsUpdating = true;
            var database = (DatabaseInfo)item.Info;

            DatabaseUpdater.Update(database,
                _ => item.IsUpdating,
                DatabaseUpdated);
        }

        private static void UpdateItem(
            DatabaseItem item, string icon)
        {
            var info = (DatabaseInfo)item.Info;

            if (info.HasPassword)
            {
                item.PasswordIcon = ThemeData
                    .GetImageSource("unlock");
            }
            else
                item.PasswordIcon = null;

            if (!string.IsNullOrEmpty(icon))
            {
                item.UpdatedIcon = ThemeData
                    .GetImageSource(icon);
            }
            else
                item.UpdatedIcon = null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_moved)
                TrialManager.CheckToastState();
        }

        private void lstDatabases_Navigation(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var item = e.Item as DatabaseItem;
            if (item == null)
                return;

            if (item.IsUpdating)
                item.IsUpdating = false;
            else
                Open((DatabaseInfo)item.Info, false);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void mnuClearKeyFile_Click(
            object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;

            database.SetKeyFile(null);
            var listItem = _items.First(
                x => x.Info == database);
            listItem.HasKeyFile = false;
        }

        private void mnuClear_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;

            database.ClearPassword();
            var listItem = _items.First(
                x => x.Info == database);

            listItem.HasPassword = false;
            listItem.PasswordIcon = null;
        }

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;

            var msg = string.Format(
                Properties.Resources.ConfirmDeleteDb,
                database.Details.Name);

            var confirm = MessageBox.Show(msg,
                Properties.Resources.DeleteDbTitle,
                MessageBoxButton.OKCancel) == MessageBoxResult.OK;

            if (!confirm)
                return;

            database.Delete();
            TilesManager.Deleted(database);

            RefreshDbList(false);
        }

        private void mnuKeyFile_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;
            this.NavigateTo<Download>(
                "folder={0}", database.Folder);
        }

        private void mnuNew_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Download>("folder=");
        }

        private void mnuPin_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;

            if (TilesManager.Pin(database))
                return;

            MessageBox.Show(
                Properties.Resources.AlreadyPinned,
                Properties.Resources.PinDatabase,
                MessageBoxButton.OK);
        }

        private void mnuRename_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;
            this.NavigateTo<Rename>("db={0}", database.Folder);
        }

        private void mnuSettings_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Settings>();
        }

        private void mnuUpdateAll_Click(object sender, EventArgs e)
        {
            var updatables = _items
                .Where(x => x.CanUpdate);

            foreach (var item in updatables)
                Update(item);
        }

        private void mnuUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!Network.CheckNetwork())
                return;

            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;

            var listItem = _items.First
                (x => x.Info == database);

            Update(listItem);
        }
    }
}