using System;
using System.Windows;
using KeePass.Data;

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

        private void cmdClearUrl_Click(object sender, RoutedEventArgs e)
        {
            cmdClearUrl.IsEnabled = false;
            AppSettingsService.DownloadUrl = string.Empty;
        }
    }
}