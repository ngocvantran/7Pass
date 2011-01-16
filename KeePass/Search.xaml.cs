using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using KeePass.Data;
using KeePass.IO;
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

        private void HideKeyboard()
        {
            if (txtSearch.Text.Length > 0)
                lstItems.Focus();
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

            var dispatcher = Dispatcher;
            entries = entries.Take(MAX_ITEMS);

            foreach (var entry in entries)
            {
                if (_wkSearch.CancellationPending)
                    return;

                _wkSearch.ReportProgress(0,
                    new GroupItem(entry, dispatcher));

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

                _wkSearch.ReportProgress(0,
                    new GroupItem(group, dispatcher));

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
            GoBack<GroupDetails>();
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            HideKeyboard();
        }

        private void lstItems_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var item = lstItems.SelectedItem as GroupItem;
            if (item == null)
                return;

            lstItems.SelectedItem = null;
            NavigationService.Navigate(item.TargetUri);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter())
                HideKeyboard();
        }

        private void txtSearch_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (_database == null)
            {
                _database = Cache.Database;

                if (_database == null)
                {
                    GoBack<MainPage>();
                    return;
                }
            }

            PerformSearch();
            _cmdSearch.IsEnabled = txtSearch.Text.Length > 0;
        }
    }
}