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
    }
}