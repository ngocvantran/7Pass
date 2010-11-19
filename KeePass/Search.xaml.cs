using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO;

namespace KeePass
{
    public partial class Search : ILifeCycleAware
    {
        private const int MAX_ITEMS = 10;

        private readonly ItemConverter _converter;
        private readonly DatabaseItems _items;
        private readonly BackgroundWorker _worker;

        private List<Entry> _entries;
        private List<Group> _groups;

        public Search()
        {
            InitializeComponent();

            DataContext = _items =
                new DatabaseItems();

            _converter = new ItemConverter();

            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
        }

        public void Load(IDictionary<string, object> states)
        {
            object text;
            if (states.TryGetValue("Search", out text))
                txtSearch.Text = (string)text;
        }

        public void Save(IDictionary<string, object> states)
        {
            states["Search"] = txtSearch.Text;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LifeCycle.CheckDbState(this);
        }

        private static bool Contains(
            string name, string condition)
        {
            return name.IndexOf(condition,
                StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void PerformSearch()
        {
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync(txtSearch.Text);
            else
                _worker.CancelAsync();
        }

        private void SortAndCacheItems()
        {
            if (_groups != null)
                return;

            var db = KeyCache.Database;

            _groups = db.Groups
                .OrderBy(x => x.Name)
                .ToList();

            _entries = db.Entries
                .OrderBy(x => x.Title)
                .ToList();
        }

        private void Home_Click(object sender, EventArgs e)
        {
            this.OpenHome();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            this.OpenSettings();
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var phrase = (string)e.Argument;
            if (string.IsNullOrEmpty(phrase))
                return;

            var conditions = phrase.Split(new[] {" "},
                StringSplitOptions.RemoveEmptyEntries);

            if (conditions.Length == 0)
                return;

            try
            {
                SortAndCacheItems();

                if (_worker.CancellationPending)
                    return;

                IEnumerable<Group> groups = _groups;
                IEnumerable<Entry> entries = _entries;

                foreach (var condition in conditions)
                {
                    var text = condition;

                    groups = groups.Where(x =>
                        Contains(x.Name, text));
                    entries = entries.Where(x =>
                        Contains(x.Title, text));
                }

                groups = groups.Take(MAX_ITEMS);
                entries = entries.Take(MAX_ITEMS);

                if (_worker.CancellationPending)
                    return;

                e.Result = _converter.Convert(groups)
                    .Union(_converter.Convert(entries))
                    .ToList();
            }
            finally
            {
                e.Cancel = _worker.CancellationPending;
            }
        }

        private void _worker_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                PerformSearch();
                return;
            }

            _items.Items = (List<DatabaseItem>)e.Result;
        }

        private void lstItems_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var index = lstItems.SelectedIndex;
            if (index < 0)
                return;

            this.NavigateTo(_items.Items[index]);
        }

        private void txtSearch_KeyDown(
            object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (txtSearch.Text.Length > 0)
                lstItems.Focus();
        }

        private void txtSearch_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            PerformSearch();
        }
    }
}