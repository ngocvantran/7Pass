using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Coding4Fun.Phone.Controls;
using KeePass.Analytics;
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

        private Group _group;
        private IList<string> _ids;
        private DateTime _lastModified;

        public GroupDetails()
        {
            InitializeComponent();

            _ids = new List<string>();
            
            _cmdHome = AppButton(0);
            _cmdHome.Text = Langs.App.Home;

            AppButton(1).Text = Langs.Group.NewEntry;
            AppButton(2).Text = Langs.Group.NewGroup;
            AppButton(3).Text = Langs.Group.Search;

            AppMenuItem(0).Text = Langs.App.SelectDb;
            AppMenuItem(1).Text = Langs.Group.ClearHistory;
            AppMenuItem(2).Text = Langs.App.About;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            var fromTile = NavigationContext
                .QueryString
                .ContainsKey("fromTile");

            if (fromTile)
                this.ClearBackStack();
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

            _group = GetGroup(database);
            lstHistory.ItemsSource = null;
            pivotGroup.Header = _group.Name;

            ThreadPool.QueueUserWorkItem(_ =>
                ListItems(_group, database.RecycleBin));

            ThreadPool.QueueUserWorkItem(_ =>
                ListHistory(database));
        }

        private static bool ConfirmDelete(
            bool pernament, string type, string name)
        {
            var message = !pernament
                ? Properties.Resources.DeletePrompt
                : Properties.Resources.DeletePernamentPrompt;

            message = string.Format(
                message, type, name);

            var confirm = MessageBox.Show(message,
                Properties.Resources.DeleteTitle,
                MessageBoxButton.OKCancel);

            return confirm == MessageBoxResult.OK;
        }

        private void Delete(Entry entry)
        {
            var database = Cache.Database;
            var pernament = IsPernamentDelete();
            AnalyticsTracker.Track(
                "modify", "delete_entry");

            if (!ConfirmDelete(pernament,
                Properties.Resources.Entry,
                entry.Title))
            {
                return;
            }

            if (!pernament)
            {
                MoveToRecycleBin((writer, recycleBin) =>
                {
                    entry.Group.Entries
                        .Remove(entry);
                    recycleBin.Add(entry);

                    writer.Location(entry);
                });
            }
            else
            {
                Save(x =>
                {
                    x.Delete(entry);
                    database.Remove(entry);
                });
            }
        }

        private void Delete(Group group)
        {
            var database = Cache.Database;
            var pernament = IsPernamentDelete();
            AnalyticsTracker.Track(
                "modify", "delete_group");

            if (!ConfirmDelete(pernament,
                Properties.Resources.Group,
                group.Name))
            {
                return;
            }

            if (!pernament)
            {
                MoveToRecycleBin((writer, recycleBin) =>
                {
                    group.Remove();
                    recycleBin.Add(group);

                    writer.Location(group);
                });
            }
            else
            {
                Save(x =>
                {
                    x.Delete(group);
                    database.Remove(group);
                });
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

        private bool IsPernamentDelete()
        {
            var database = Cache.Database;

            if (!database.RecycleBinEnabled)
                return true;

            var recycleBin = database.RecycleBin;
            if (recycleBin != null && recycleBin == _group)
                return true;

            return false;
        }

        private bool IsSameData(ICollection<Group> groups,
            ICollection<Entry> entries)
        {
            var lastModified = groups
                .Select(x => x.LastModified)
                .Union(entries.Select(x => x.LastModified))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            var ids = groups
                .Select(x => x.ID)
                .Union(entries.Select(x => x.ID))
                .ToList();

            var sameIds = ids.Intersect(_ids)
                .Count() == ids.Count;

            if (sameIds && lastModified == _lastModified)
                return true;

            _ids = ids;
            _lastModified = lastModified;

            return false;
        }

        private void ListHistory(Database database)
        {
            var dispatcher = Dispatcher;

            var recents = Cache.GetRecents()
                .Select(database.GetEntry)
                .Where(x => x != null)
                .Select(x => new GroupItem(x, dispatcher))
                .ToList();

            lstHistory.SetItems(recents);
        }

        private void ListItems(Group group, Group recycleBin)
        {
            var dispatcher = Dispatcher;
            var groups = group.Groups.ToList();

            if (recycleBin != null)
            {
                var settings = AppSettings.Instance;

                if (settings.HideRecycleBin)
                    groups.Remove(recycleBin);
            }

            if (IsSameData(groups, group.Entries))
                return;

            var items = new List<GroupItem>();
            items.AddRange(groups
                .OrderBy(x => x.Name)
                .Select(x => new GroupItem(x, dispatcher)));
            items.AddRange(group.Entries
                .OrderBy(x => x.Title)
                .Select(x => new GroupItem(x, dispatcher)));

            lstGroup.SetItems(items);
        }

        private void MoveToRecycleBin(
            Action<DatabaseWriter, Group> action)
        {
            Save(x =>
            {
                var database = Cache.Database;

                var recycleBin = database.RecycleBin;
                if (recycleBin == null)
                {
                    recycleBin = database.AddNew(_group,
                        Properties.Resources.RecycleBin);

                    recycleBin.Icon = new IconData
                    {
                        Standard = 43,
                    };

                    x.New(recycleBin);
                    database.RecycleBin = recycleBin;
                }

                action(x, recycleBin);
            });
        }

        private void Save(Action<DatabaseWriter> save)
        {
            IsEnabled = false;

            var info = Cache.DbInfo;
            var database = Cache.Database;
            var writer = new DatabaseWriter();

            info.OpenDatabaseFile(x => writer
                .LoadExisting(x, info.Data.MasterKey));

            save(writer);
            info.SetDatabase(x => writer.Save(
                x, database.RecycleBin));

            IsEnabled = true;
            ThreadPool.QueueUserWorkItem(_ => ListItems(
                _group, Cache.Database.RecycleBin));

            Cache.UpdateRecents();
            lstHistory.ItemsSource = null;

            ThreadPool.QueueUserWorkItem(_ =>
                ListHistory(database));

            Dispatcher.BeginInvoke(() =>
                info.NotifyIfNotSyncable());
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            this.BackToRoot();
        }

        private void cmdNewEntry_Click(object sender, EventArgs e)
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

        private void cmdNewGroup_Click(object sender, EventArgs e)
        {
            var dlgNewGroup = new InputPrompt
            {
                Message = Properties.Resources.PromptName,
                Title = Properties.Resources.NewGroupTitle,
            };
            dlgNewGroup.Completed += dlgNewGroup_Completed;

            dlgNewGroup.Show();
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

            AnalyticsTracker.Track(
                "modify", "new_group");
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

            AnalyticsTracker.Track(
                "modify", "rename_group");
        }

        private void lstGroup_Navigation(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var item = e.Item as GroupItem;
            if (item == null)
                return;

            NavigationService.Navigate(item.TargetUri);
        }

        private void lstHistory_SelectionChanged(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var item = e.Item as GroupItem;
            if (item == null)
                return;

            NavigationService.Navigate(item.TargetUri);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            var mnuDelete = (MenuItem)sender;
            var entry = mnuDelete.Tag as Entry;

            if (entry != null)
            {
                Delete(entry);

                return;
            }

            Delete((Group)mnuDelete.Tag);
        }

        private void mnuHistory_Click(object sender, EventArgs e)
        {
            Cache.ClearRecents();
            lstHistory.ItemsSource = null;
        }

        private void mnuMove_Click(object sender, RoutedEventArgs e)
        {
            var mnuMove = (MenuItem)sender;

            var entry = mnuMove.Tag as Entry;
            if (entry != null)
            {
                this.NavigateTo<MoveTarget>(
                    "entry={0}", entry.ID);

                return;
            }

            var group = (Group)mnuMove.Tag;
            this.NavigateTo<MoveTarget>(
                "group={0}", group.ID);
        }

        private void mnuRename_Click(object sender, RoutedEventArgs e)
        {
            var mnuRename = (MenuItem)sender;
            var group = (Group)mnuRename.Tag;

            var dlgRename = new InputPrompt
            {
                Tag = group,
                Value = group.Name,
                Title = Properties.Resources.RenameTitle,
                Message = Properties.Resources.PromptName,
            };

            dlgRename.Completed += dlgRename_Completed;

            dlgRename.Show();
        }

        private void mnuRoot_Click(object sender, EventArgs e)
        {
            this.BackToDBs();
        }

        private void mnuSearch_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Search>();
        }
    }
}