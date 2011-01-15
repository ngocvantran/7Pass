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
        private bool _loaded;

        public GroupDetails()
        {
            InitializeComponent();

            _cmdHome = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];

            _items = new ObservableCollection<GroupItem>();
            lstGroup.ItemsSource = _items;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled || _loaded)
                return;

            var database = Cache.Database;
            if (database == null)
            {
                GoBack<MainPage>();
                return;
            }

            _loaded = true;
            ListItems(database);
        }

        private void Display(object itemsList)
        {
            var dispatcher = Dispatcher;
            var items = (IList<GroupItem>)itemsList;

            foreach (var item in items)
            {
                var local = item;
                dispatcher.BeginInvoke(() =>
                    _items.Add(local));

                Thread.Sleep(50);
            }
        }

        private Group GetGroup(Database database)
        {
            string groupId;
            var queries = NavigationContext.QueryString;

            Group group;
            if (queries.TryGetValue("id", out groupId))
                group = database.GetGroup(groupId);
            else
            {
                group = database.Root;
                _cmdHome.IsEnabled = false;
            }
            return group;
        }

        private void ListItems(Database database)
        {
            var group = GetGroup(database);

            PageTitle.Text = group.Name;

            var items = new List<GroupItem>();
            items.AddRange(group.Groups
                .OrderBy(x => x.Name)
                .Select(x => new GroupItem(x)));
            items.AddRange(group.Entries
                .OrderBy(x => x.Title)
                .Select(x => new GroupItem(x)));

            ThreadPool.QueueUserWorkItem(Display, items);
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            GoBack<GroupDetails>();
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Search>();
        }

        private void lstGroup_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var item = lstGroup.SelectedItem as GroupItem;
            if (item == null)
                return;

            NavigationService.Navigate(item.TargetUri);
            lstGroup.SelectedItem = null;
        }
    }
}