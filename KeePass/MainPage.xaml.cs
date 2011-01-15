using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
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
            if (!cancelled)
                RefreshDbList();
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
                    _items.Add(local));

                Thread.Sleep(50);
            }
        }

        private void RefreshDbList()
        {
            _items.Clear();
            new Thread(ListDatabases)
                .Start();
        }

        private void ctmDatabase_Opened(object sender, RoutedEventArgs e)
        {
            var context = (ContextMenu)sender;
            var database = (DatabaseInfo)context.Tag;
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
        }
    }
}