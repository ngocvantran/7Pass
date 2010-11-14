using System;
using System.Windows;

namespace KeePass
{
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();

            cmdClearPass.IsEnabled = KeyCache.StorePassword;
        }

        private void DownloadDatabase(object sender, RoutedEventArgs e)
        {
            this.OpenDownload();
        }

        private void cmdClearPass_Click(object sender, RoutedEventArgs e)
        {
            KeyCache.StorePassword = false;
            cmdClearPass.IsEnabled = false;
        }
    }
}