using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Navigation;
using KeePass.Analytics;
using KeePass.Data;
using KeePass.IO.Data;
using KeePass.IO.Write;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class MoveTarget
    {
        private readonly ApplicationBarIconButton _cmdMove;

        private Database _database;
        private Entry _entry;
        private Group _group;
        private Group _target;

        public MoveTarget()
        {
            InitializeComponent();

            _cmdMove = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            _database = Cache.Database;
            if (_database == null)
            {
                this.BackToDBs();
                return;
            }

            string id;
            var queries = NavigationContext.QueryString;

            if (queries.TryGetValue("entry", out id))
                _entry = _database.GetEntry(id);
            else
            {
                id = queries["group"];
                _group = _database.GetGroup(id);
            }

            _target = _database.Root;
            Refresh();
        }

        private static string GetPath(Group group)
        {
            var sb = new StringBuilder(
                group.Name);

            while (group.Parent != null)
            {
                group = group.Parent;

                sb.Insert(0, " » ");
                sb.Insert(0, group.Name);
            }

            return sb.ToString();
        }

        private void Refresh()
        {
            SetMoveState();
            lblTarget.Text = GetPath(_target);

            var dispatcher = Dispatcher;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var groups = _target.Groups
                    .ToList();

                if (_group != null)
                    groups.Remove(_group);

                var recycleBin = _database
                    .RecycleBin;

                if (recycleBin != null)
                    groups.Remove(recycleBin);

                var items = new List<GroupItem>();

                items.AddRange(groups
                    .OrderBy(x => x.Name)
                    .Select(x => new GroupItem(
                        x, dispatcher)));

                if (_target.Parent != null)
                {
                    items.Insert(0, new GroupItem(
                        _target.Parent, dispatcher)
                    {
                        Icon = ThemeData.GetImage("up"),
                    });
                }

                lstGroup.SetItems(items);
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
        }

        private void SetMoveState()
        {
            var current = _group != null
                ? _group.Parent : _entry.Group;

            _cmdMove.IsEnabled =
                _target != current;
        }

        private void cmdMove_Click(object sender, EventArgs e)
        {
            if (_entry != null)
            {
                Save(x =>
                {
                    _entry.Remove();
                    _target.Add(_entry);
                    x.Location(_entry);
                });
                
                AnalyticsTracker.Track(
                    "modify", "move_entry");
            }
            else
            {
                Save(x =>
                {
                    _group.Remove();
                    _target.Add(_group);
                    x.Location(_group);
                });

                AnalyticsTracker.Track(
                    "modify", "move_group");
            }

            NavigationService.GoBack();
        }

        private void lst_SelectionChanged(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var item = e.Item as GroupItem;
            if (item == null)
                return;

            _target = (Group)item.Data;
            Refresh();
        }
    }
}