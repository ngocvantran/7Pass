using System;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Properties;
using KeePass.Services;

namespace KeePass
{
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();

            if (!TrialManager.IsTrial)
                return;

            txtTrial.Visibility = Visibility.Visible;
            lnkTrial.Visibility = Visibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            cmdClearPass.IsEnabled =
                AppSettingsService.HasPassword();
            cmdClearUrl.IsEnabled = !string.IsNullOrEmpty(
                AppSettingsService.DownloadUrl);
            cmdClearRecents.IsEnabled = AppSettingsService
                .GetRecents().Count > 0;

            txtTrial.Text = string.Format(AppResources.Trial,
                TrialManager.Usages, TrialManager.USAGES_LIMIT);
        }

        private void DownloadDatabase(object sender, RoutedEventArgs e)
        {
            this.OpenDownload();
        }

        private void cmdClearPass_Click(object sender, RoutedEventArgs e)
        {
            cmdClearPass.IsEnabled = false;
            AppSettingsService.ClearPassword();
        }

        private void cmdClearRecents_Click(object sender, RoutedEventArgs e)
        {
            cmdClearRecents.IsEnabled = false;
            AppSettingsService.ClearRecents();
        }

        private void cmdClearUrl_Click(object sender, RoutedEventArgs e)
        {
            cmdClearUrl.IsEnabled = false;
            AppSettingsService.DownloadUrl = string.Empty;
        }

        private void lnkTrial_Click(object sender, RoutedEventArgs e)
        {
            TrialManager.ShowPurchase();
        }
    }
}