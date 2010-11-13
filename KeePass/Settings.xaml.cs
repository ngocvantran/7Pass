using System;
using System.Windows;

namespace KeePass
{
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void DownloadDatabase(object sender, RoutedEventArgs e)
        {
            this.OpenDownload();
        }
    }
}