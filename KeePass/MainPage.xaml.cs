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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (LifeCycle.CheckDbState(this) ==
                DatabaseCheckResults.Terminate)
            {
                return;
            }

            // Do not reload data
            if (lstItems.Items.Count > 0)
                return;

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
            PageTitle.Text = group.Name;

            var args = new WorkerArgs
            {
                Root = group,
                Converter = new ItemConverter(),
                Items = _items.Items =
                    new ObservableCollection<DatabaseItem>(),
            };

            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync(args);
        }

        private Group GetGroup()
        {
            string groupId;
            var db = KeyCache.Database;
            var pars = NavigationContext.QueryString;

            return !pars.TryGetValue("id", out groupId)
                ? db.Root : db.GetGroup(new Guid(groupId));
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

        private void lstItems_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var index = lstItems.SelectedIndex;
            if (index < 0)
                return;

            this.NavigateTo(_items.Items[index]);
            lstItems.SelectedIndex = -1;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (WorkerArgs)e.Argument;

            var root = args.Root;
            var list = args.Items;
            var converter = args.Converter;

            var children = converter.Convert(root.Groups)
                .Union(converter.Convert(root.Entries))
                .ToList();

            var dispatcher = lstItems.Dispatcher;
            foreach (var child in children)
            {
                var item = child;
                dispatcher.BeginInvoke(
                    () => list.Add(item));

                Thread.Sleep(50);
            }
        }

        private class WorkerArgs
        {
            public ItemConverter Converter { get; set; }
            public IList<DatabaseItem> Items { get; set; }
            public Group Root { get; set; }
        }
    }
}