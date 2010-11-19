using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO;

namespace KeePass
{
    public partial class EntryPage
    {
        public EntryPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (LifeCycle.CheckDbState(this) ==
                DatabaseCheckResults.Terminate)
            {
                return;
            }

            var entry = GetEntry();
            DataContext = entry;

            var others = entry.GetOthers();
            if (others.Length == 0)
                return;

            var rows = gridMain.RowDefinitions;
            for (var i = 0; i < others.Length; i++)
            {
                rows.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });
            }

            var children = gridMain.Children;
            for (var i = 0; i < others.Length; i++)
            {
                var key = others[i];

                var text = new TextBlock
                {
                    Text = key
                };
                var value = new TextBox
                {
                    Text = entry[key],
                    IsReadOnly = true,
                };

                Grid.SetColumn(text, 0);
                Grid.SetColumn(value, 1);

                Grid.SetRow(text, 4 + i);
                Grid.SetRow(value, 4 + i);

                children.Add(text);
                children.Add(value);
            }
        }

        private Entry GetEntry()
        {
            var db = KeyCache.Database;
            var entryId = NavigationContext.QueryString["id"];

            return db.GetEntry(new Guid(entryId));
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
    }
}