using System;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Utils;

namespace KeePass
{
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var settings = AppSettings.Instance;
            chkBrowser.IsChecked = settings.UseIntBrowser;
            chkRecycleBin.IsChecked = settings.HideRecycleBin;

            chkPassword.IsChecked = !string
                .IsNullOrEmpty(settings.Password);
        }

        private void chkBrowser_CheckedChanged(
            object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.UseIntBrowser =
                chkBrowser.IsChecked == true;
        }

        private void chkPassword_Checked(
            object sender, RoutedEventArgs e)
        {
            this.NavigateTo<GlobalPass>();
        }

        private void chkPassword_Loaded(object sender, RoutedEventArgs e)
        {
            chkPassword.Checked += chkPassword_Checked;
            chkPassword.Unchecked += chkPassword_Unchecked;
        }

        private static void chkPassword_Unchecked(
            object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.Password = string.Empty;
        }

        private void chkRecycleBin_CheckedChanged(
            object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.HideRecycleBin =
                chkRecycleBin.IsChecked == true;
        }
    }
}