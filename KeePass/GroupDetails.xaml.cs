using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Coding4Fun.Phone.Controls;
using KeePass.Data;
using KeePass.IO.Data;
using KeePass.IO.Write;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class GroupDetails
    {
        private readonly ApplicationBarIconButton _cmdHome;
        private readonly ObservableCollection<GroupItem> _items;
        private readonly ObservableCollection<GroupItem> _recents;

        private Group _group;

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
                GoBack<MainPage>();
                return;
            }

            _group = GetGroup(database);
            pivotGroup.Header = _group.Name;

            ThreadPool.QueueUserWorkItem(_ =>
                ListItems(_group, database.RecycleBin));

            _recents.Clear();
            ThreadPool.QueueUserWorkItem(_ =>
                ListHistory(database));
        }

        private void Display(
            IEnumerable<GroupItem> items,
            ICollection<GroupItem> list)
        {
            var dispatcher = Dispatcher;
            dispatcher.BeginInvoke(list.Clear);

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

        private void Save(Action<DatabaseWriter> save)
        {
            IsEnabled = false;

            var info = Cache.DbInfo;
            var writer = new DatabaseWriter();

            info.OpenDatabaseFile(x => writer
                .LoadExisting(x, info.Data.MasterKey));

            save(writer);
            info.SetDatabase(writer.Save);

            IsEnabled = true;
            ThreadPool.QueueUserWorkItem(_ => ListItems(
                _group, Cache.Database.RecycleBin));
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

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Search>();
        }

        private void dlgNewGroup_Completed(object sender,
            PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult != PopUpResult.Ok)
                return;

            if (string.IsNullOrEmpty(e.Result))
                return;

            Save(x =>
            {
                var database = Cache.Database;

                var group = database
                    .AddNew(_group, e.Result);

                x.New(group);
            });
        }

        private void dlgRename_Completed(object sender,
            PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult != PopUpResult.Ok)
                return;

            if (string.IsNullOrEmpty(e.Result))
                return;

            var dlgRename = (InputPrompt)sender;
            var group = (Group)dlgRename.Tag;

            Save(x =>
            {
                group.Name = e.Result;
                x.Details(group);
            });
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

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            var mnuDelete = (MenuItem)sender;
            // TODO: Delete
        }

        private void mnuHistory_Click(object sender, EventArgs e)
        {
            _recents.Clear();
            Cache.ClearRecents();
        }

        private void mnuNewEntry_Click(object sender, EventArgs e)
        {
            string groupId;
            var queries = NavigationContext.QueryString;

            if (!queries.TryGetValue("id", out groupId) ||
                groupId == null)
            {
                groupId = string.Empty;
            }

            this.NavigateTo<EntryDetails>(
                "group={0}", groupId);
        }

        private void mnuNewGroup_Click(object sender, EventArgs e)
        {
            var dlgNewGroup = new InputPrompt
            {
                Title = "New Group",
                Message = "Please enter group name"
            };
            dlgNewGroup.Completed += dlgNewGroup_Completed;

            dlgNewGroup.Show();
        }

        private void mnuRename_Click(object sender, RoutedEventArgs e)
        {
            var mnuRename = (MenuItem)sender;
            var group = (Group)mnuRename.Tag;

            var dlgRename = new InputPrompt
            {
                Tag = group,
                Title = "Rename",
                Value = group.Name,
                IsCancelVisible = true,
                Message = "Please enter group name",
            };

            dlgRename.Completed += dlgRename_Completed;

            dlgRename.Show();
        }
    }
}