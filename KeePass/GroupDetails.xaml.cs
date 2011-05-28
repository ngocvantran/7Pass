using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class GroupDetails
    {
        private readonly ApplicationBarIconButton _cmdHome;
        private readonly ObservableCollection<GroupItem> _items;
        private readonly ObservableCollection<GroupItem> _recents;

        private bool _loaded;

        public GroupDetails()
        {
            InitializeComponent();

            _cmdHome = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];

            _items = new ObservableCollection<GroupItem>();
            _recents = new ObservableCollection<GroupItem>();

            lstGroup.ItemsSource = _items;
            lstHistory.ItemsSource = _recents;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var database = Cache.Database;
            if (database == null)
            {
                this.BackToDatabases();
                return;
            }

            if (!_loaded)
            {
                _loaded = true;
                var group = GetGroup(database);
                pivotGroup.Header = group.Name;

                ThreadPool.QueueUserWorkItem(_ =>
                    ListItems(group, database.RecycleBin));
            }

            _recents.Clear();
            ThreadPool.QueueUserWorkItem(_ =>
                ListHistory(database));
        }

        private void Display(
            IEnumerable<GroupItem> items,
            ICollection<GroupItem> list)
        {
            var dispatcher = Dispatcher;

            foreach (var item in items)
            {
                var local = item;
                dispatcher.BeginInvoke(() =>
                    list.Add(local));

                Thread.Sleep(50);
            }
        }

        private Group GetGroup(Database database)
        {
            string groupId;
            var queries = NavigationContext.QueryString;

            if (queries.TryGetValue("id", out groupId))
                return database.GetGroup(groupId);

            _cmdHome.IsEnabled = false;
            return database.Root;
        }

        private void ListHistory(Database database)
        {
            var dispatcher = Dispatcher;

            var recents = Cache.GetRecents()
                .Select(database.GetEntry)
                .Select(x => new GroupItem(x, dispatcher))
                .ToList();

            if (recents.Count > 0)
                Display(recents, _recents);
        }

        private void ListItems(Group group, Group recycleBin)
        {
            var dispatcher = Dispatcher;
            var groups = group.Groups.AsEnumerable();

            if (recycleBin != null)
            {
                var settings = AppSettings.Instance;

                if (settings.HideRecycleBin)
                    groups = groups.Except(new[] {recycleBin});
            }

            var items = new List<GroupItem>();
            items.AddRange(groups
                .OrderBy(x => x.Name)
                .Select(x => new GroupItem(x, dispatcher)));
            items.AddRange(group.Entries
                .OrderBy(x => x.Title)
                .Select(x => new GroupItem(x, dispatcher)));

            if (items.Count > 0)
                Display(items, _items);
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            this.BackToRoot();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            this.BackToDatabases();
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Search>();
        }

        private void lst_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;

            var item = list.SelectedItem as GroupItem;
            if (item == null)
                return;

            NavigationService.Navigate(item.TargetUri);
            list.SelectedItem = null;
        }

        private void mnuHistory_Click(object sender, EventArgs e)
        {
            _recents.Clear();
            Cache.ClearRecents();
        }
    }
}