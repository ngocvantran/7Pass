using System;
using System.ComponentModel;
using System.Windows;
using KeePass.Properties;
using KeePass.Services;

namespace KeePass
{
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();

            cmdClearPass.IsEnabled =
                AppSettingsService.HasPassword();
            cmdClearUrl.IsEnabled = !string.IsNullOrEmpty(
                AppSettingsService.DownloadUrl);

            if (!TrialManager.IsTrial)
                return;

            txtTrial.Visibility = Visibility.Visible;
            lnkTrial.Visibility = Visibility.Visible;

            txtTrial.Text = string.Format(AppResources.Trial,
                TrialManager.Usages, TrialManager.USAGES_LIMIT);
        }

        private void DownloadDatabase(object sender, RoutedEventArgs e)
        {
            this.OpenDownload();
        }

        private void PhoneApplicationPage_BackKeyPress(
            object sender, CancelEventArgs e)
        {
            if (!TrialManager.HasExpired)
                return;

            e.Cancel = true;
            App.Quit();
        }

        private void cmdClearPass_Click(object sender, RoutedEventArgs e)
        {
            cmdClearPass.IsEnabled = false;
            AppSettingsService.ClearPassword();
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