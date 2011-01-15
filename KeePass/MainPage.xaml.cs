using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.Sources;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Controls;

namespace KeePass
{
    public partial class MainPage
    {
        private readonly ObservableCollection<DatabaseItem> _items;

        public MainPage()
        {
            InitializeComponent();

            _items = new ObservableCollection<DatabaseItem>();
            lstDatabases.ItemsSource = _items;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            Cache.Clear();
            RefreshDbList();
        }

        private void DatabaseUpdated(
            DatabaseInfo info, bool success)
        {
            var dispatcher = Dispatcher;
            var listItem = _items.First(
                x => x.Info == info);

            dispatcher.BeginInvoke(() =>
                listItem.IsUpdating = false);

            if (success)
                return;

            var msg = string.Format(
                Properties.Resources.UpdateFailure,
                info.Details.Name);

            dispatcher.BeginInvoke(() =>
                MessageBox.Show(msg,
                    Properties.Resources.UpdateTitle,
                    MessageBoxButton.OK));
        }

        private void ListDatabases(object ignored)
        {
            var dispatcher = Dispatcher;

            var items = DatabaseInfo.GetAll()
                .Select(x => new DatabaseItem(x))
                .OrderBy(x => x.Name)
                .ToList();

            foreach (var item in items)
            {
                var local = item;
                dispatcher.BeginInvoke(() =>
                {
                    if (local.HasPassword)
                    {
                        local.PasswordIcon = ThemeData
                            .GetImageSource("unlock");
                    }

                    _items.Add(local);
                });

                Thread.Sleep(50);
            }
        }

        private void RefreshDbList()
        {
            _items.Clear();
            ThreadPool.QueueUserWorkItem(ListDatabases);
        }

        private void lstDatabases_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var item = lstDatabases.SelectedItem as DatabaseItem;
            if (item == null)
                return;

            if (item.IsUpdating)
                item.IsUpdating = false;
            else
            {
                var database = (DatabaseInfo)item.Info;

                if (!database.HasPassword)
                {
                    this.NavigateTo<Password>("db={0}",
                        database.Folder);
                }
                else
                {
                    database.Open(Dispatcher);
                    this.NavigateTo<GroupDetails>();
                }
            }

            lstDatabases.SelectedItem = null;
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
            RefreshDbList();
        }

        private void mnuNew_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Download>();
        }

        private void mnuUpdate_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var database = (DatabaseInfo)item.Tag;

            var listItem = _items.First(x => x.Info == database);
            listItem.IsUpdating = true;

            Sources.DropBox.DropBoxUpdater.Update(database,
                _ => listItem.IsUpdating, DatabaseUpdated);
        }
    }
}