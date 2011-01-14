using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using KeePass.Sources;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class MainPage
    {
        private readonly ObservableCollection<DatabaseListItemInfo> _items;

        public MainPage()
        {
            InitializeComponent();

            _items = new ObservableCollection<DatabaseListItemInfo>();
            lstDatabases.ItemsSource = _items;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            _items.Clear();
            new Thread(ListDatabases).Start();
        }

        private void ListDatabases(object ignored)
        {
            var dispatcher = Dispatcher;

            var items = DatabaseInfo.GetAll()
                .Select(x => new DatabaseListItemInfo(x))
                .ToList();

            foreach (var item in items)
            {
                var local = item;
                dispatcher.BeginInvoke(() =>
                    _items.Add(local));

                Thread.Sleep(50);
            }
        }

        private void mnuNew_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Download>();
        }
    }
}