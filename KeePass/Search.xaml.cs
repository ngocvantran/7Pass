using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using KeePass.Analytics;
using KeePass.Controls;
using KeePass.Data;
using KeePass.IO.Data;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class Search
    {
        private const int MAX_ITEMS = 10;

        private readonly ApplicationBarIconButton _cmdSearch;
        private readonly ObservableCollection<GroupItem> _items;
        private readonly BackgroundWorker _wkSearch;

        private Database _database;

        public Search()
        {
            InitializeComponent();

            _items = new ObservableCollection<GroupItem>();
            lstItems.ItemsSource = _items;

            _cmdSearch = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];

            _wkSearch = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            _wkSearch.DoWork += _wkSearch_DoWork;
            _wkSearch.ProgressChanged += _wkSearch_ProgressChanged;
            _wkSearch.RunWorkerCompleted += _wkSearch_RunWorkerCompleted;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            State["Search"] = txtSearch.Text;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            if (Cache.Database == null)
                this.BackToDBs();

            object search;
            if (!State.TryGetValue("Search", out search))
                return;

            var searchText = search as string;
            if (!string.IsNullOrEmpty(searchText))
                txtSearch.Text = searchText;

            AnalyticsTracker.Track("search");
        }

        private void HideKeyboard()
        {
            if (txtSearch.Text.Length > 0)
                lstItems.Focus();
        }

        private static bool IsRelated(Group group, Group target)
        {
            if (group == target)
                return true;

            var parent = group.Parent;
            if (parent == null)
                return false;

            return IsRelated(parent, target);
        }

        private void PerformSearch()
        {
            if (!_wkSearch.IsBusy)
            {
                _items.Clear();
                _wkSearch.RunWorkerAsync(txtSearch.Text);
            }
            else
                _wkSearch.CancelAsync();
        }

        private void SearchEntries(IEnumerable<string> conditions)
        {
            var entries = _database.Entries;
            foreach (var condition in conditions)
            {
                var local = condition;
                entries = entries.Where(x =>
                    x.Title.ToUpperInvariant()
                        .Contains(local));
            }

            if (_wkSearch.CancellationPending)
                return;

            var recycleBin = _database.RecycleBin;
            if (recycleBin != null && AppSettings
                .Instance.HideRecycleBin)
            {
                entries = entries.Where(x =>
                    !IsRelated(x.Group, recycleBin));
            }

            if (_wkSearch.CancellationPending)
                return;

            var dispatcher = Dispatcher;
            entries = entries.Take(MAX_ITEMS);

            foreach (var entry in entries)
            {
                if (_wkSearch.CancellationPending)
                    return;

                var sb = new StringBuilder();
                entry.Group.GetPath(sb, "»", true);

                _wkSearch.ReportProgress(0,
                    new GroupItem(entry, dispatcher)
                    {
                        Notes = sb.ToString(),
                    });

                Thread.Sleep(50);
            }
        }

        private void SearchGroups(IEnumerable<string> conditions)
        {
            var groups = _database.Groups;
            foreach (var condition in conditions)
            {
                var local = condition;
                groups = groups.Where(x =>
                    x.Name.ToUpperInvariant()
                        .Contains(local));
            }

            if (_wkSearch.CancellationPending)
                return;

            var dispatcher = Dispatcher;
            groups = groups.Take(MAX_ITEMS);

            foreach (var group in groups)
            {
                if (_wkSearch.CancellationPending)
                    return;

                var sb = new StringBuilder();
                group.GetPath(sb, "»", false);

                _wkSearch.ReportProgress(0,
                    new GroupItem(group, dispatcher)
                    {
                        Notes = sb.ToString(),
                    });

                Thread.Sleep(50);
            }
        }

        private void _wkSearch_DoWork(
            object sender, DoWorkEventArgs e)
        {
            var query = (string)e.Argument;

            var conditions = query.Split(new[] {" "},
                StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToUpperInvariant())
                .ToList();

            if (conditions.Count == 0)
                return;

            try
            {
                SearchEntries(conditions);
                SearchGroups(conditions);
            }
            finally
            {
                e.Cancel = _wkSearch.CancellationPending;
            }
        }

        private void _wkSearch_ProgressChanged(
            object sender, ProgressChangedEventArgs e)
        {
            _items.Add((GroupItem)e.UserState);
        }

        private void _wkSearch_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                PerformSearch();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            this.BackToRoot();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            this.BackToDBs();
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            HideKeyboard();
        }

        private void lstItems_SelectionChanged(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var item = e.Item as GroupItem;
            if (item == null)
                return;

            NavigationService.Navigate(item.TargetUri);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter())
                HideKeyboard();
        }

        private void txtSearch_Loaded(object sender, RoutedEventArgs e)
        {
            txtSearch.Focus();
        }

        private void txtSearch_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (_database == null)
            {
                _database = Cache.Database;

                if (_database == null)
                {
                    this.BackToDBs();
                    return;
                }
            }

            PerformSearch();
            _cmdSearch.IsEnabled = txtSearch.Text.Length > 0;
        }
    }
}