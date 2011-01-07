using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.IO;
using KeePass.Properties;
using KeePass.Services;

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

            LoadData();
            CheckLicense();
        }

        private static void CheckLicense()
        {
            if (!TrialManager.HasExpired)
                return;

            var purchase = MessageBox.Show(
                AppResources.NotifyPurchse,
                AppResources.EndOfTrial,
                MessageBoxButton.OKCancel) ==
                    MessageBoxResult.OK;

            if (purchase)
                TrialManager.ShowPurchase();
        }

        private Entry GetEntry()
        {
            var db = KeyCache.Database;
            var entryId = NavigationContext.QueryString["id"];

            return db.GetEntry(entryId);
        }

        private void LoadData()
        {
            if (DataContext != null)
                return;

            var entry = GetEntry();
            if (entry == null)
            {
                NavigationService.GoBack();
                return;
            }

            DataContext = entry;
            AppSettingsService
                .AddRecent(entry.ID);

            var others = entry.GetOthers();
            if (others.Length == 0)
                return;

            var rows = gridFields.RowDefinitions;
            for (var i = 0; i < others.Length; i++)
            {
                rows.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });
            }

            var children = gridFields.Children;
            for (var i = 0; i < others.Length; i++)
            {
                var key = others[i];

                var text = new TextBlock
                {
                    Text = key,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var value = new TextBox
                {
                    Text = entry[key],
                    IsReadOnly = true,
                };

                Grid.SetColumn(text, 0);
                Grid.SetColumn(value, 1);

                Grid.SetRow(text, i);
                Grid.SetRow(value, i);

                children.Add(text);
                children.Add(value);
            }
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