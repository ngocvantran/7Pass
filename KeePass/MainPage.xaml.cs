using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO;
using KeePass.Services;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class MainPage
    {
        private readonly DatabaseItems _items;

        public MainPage()
        {
            InitializeComponent();

            DataContext = _items =
                new DatabaseItems();
        }

        protected override void OnNavigatedTo(
            NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (LifeCycle.CheckDbState(this) ==
                DatabaseCheckResults.Terminate)
            {
                return;
            }

            var converter = new ItemConverter(
                KeyCache.Database);

            if (lstItems.Items.Count == 0)
                LoadGroupItems(converter);

            LoadHistoryItems(converter);
        }

        private Group GetGroup()
        {
            string groupId;
            var db = KeyCache.Database;
            var pars = NavigationContext.QueryString;

            return !pars.TryGetValue("id", out groupId)
                ? db.Root : db.GetGroup(new Guid(groupId));
        }

        private void LoadGroupItems(ItemConverter converter)
        {
            var group = GetGroup();
            if (group == null)
            {
                this.OpenHome();
                return;
            }

            var home = ((ApplicationBarIconButton)
                ApplicationBar.Buttons[0]);

            home.IsEnabled = NavigationContext
                .QueryString.Count != 0;

            // Display entries
            pivotGroup.Header = group.Name;
            _items.Items = new ObservableCollection<DatabaseItem>();

            var bwGroup = new BackgroundWorker();
            bwGroup.DoWork += bwGroup_DoWork;
            bwGroup.RunWorkerAsync(new GroupWorkerArgs
            {
                Root = group,
                Items = _items.Items,
                Converter = converter,
            });
        }

        private void LoadHistoryItems(ItemConverter converter)
        {
            _items.Histories = new ObservableCollection<DatabaseItem>();

            var bwHistory = new BackgroundWorker();
            bwHistory.DoWork += bwHistory_DoWork;
            bwHistory.RunWorkerAsync(new HistoryWorkerArgs
            {
                Converter = converter,
                Items = _items.Histories,
            });
        }

        private void Home_Click(object sender, EventArgs e)
        {
            this.OpenHome();
        }

        private void Search_Click(object sender, EventArgs e)
        {
            this.OpenSearch();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            this.OpenSettings();
        }

        private void bwGroup_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (GroupWorkerArgs)e.Argument;

            var root = args.Root;
            var list = args.Items;
            var dispatcher = Dispatcher;
            var converter = args.Converter;

            var children = converter.Convert(root.Groups, dispatcher)
                .Union(converter.Convert(root.Entries, dispatcher))
                .ToList();

            foreach (var child in children)
            {
                var item = child;
                dispatcher.BeginInvoke(
                    () => list.Add(item));

                Thread.Sleep(50);
            }
        }

        private void bwHistory_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (HistoryWorkerArgs)e.Argument;

            var list = args.Items;
            var db = KeyCache.Database;
            var dispatcher = Dispatcher;
            var converter = args.Converter;

            var recents = AppSettingsService.GetRecents()
                .Select(db.GetEntry);

            var entries = converter.Convert(
                recents, dispatcher)
                .ToList();

            foreach (var entry in entries)
            {
                var item = entry;
                dispatcher.BeginInvoke(
                    () => list.Add(item));

                Thread.Sleep(50);
            }
        }

        private void lstHistory_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var index = lstHistory.SelectedIndex;
            if (index < 0)
                return;

            this.NavigateTo(_items.Histories[index]);
            lstHistory.SelectedIndex = -1;
        }

        private void lstItems_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var index = lstItems.SelectedIndex;
            if (index < 0)
                return;

            this.NavigateTo(_items.Items[index]);
            lstItems.SelectedIndex = -1;
        }

        private class GroupWorkerArgs
        {
            public ItemConverter Converter { get; set; }
            public IList<DatabaseItem> Items { get; set; }
            public Group Root { get; set; }
        }

        private class HistoryWorkerArgs
        {
            public ItemConverter Converter { get; set; }
            public IList<DatabaseItem> Items { get; set; }
        }
    }
}