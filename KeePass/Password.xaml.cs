using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KeePass.Data;
using KeePass.Properties;
using KeePass.Services;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class Password
    {
        private readonly ApplicationBarIconButton _cmdAppBarOpen;

        public Password()
        {
            InitializeComponent();

            _cmdAppBarOpen = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];

            if (Theme.IsDarkTheme())
                return;

            var uri = new Uri("/Images/warning.light.png",
                UriKind.Relative);

            imgWarning.Source = new BitmapImage(uri);
        }

        private void OpenDatabase()
        {
            var opened = AppSettingsService.Open(
                txtPassword.Password,
                chkStore.IsChecked == true);

            if (opened)
            {
                NavigationService.GoBack();
                return;
            }

            MessageBox.Show(AppResources.IncorrectPassword,
                AppResources.PasswordTitle,
                MessageBoxButton.OK);
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            this.OpenSettings();
        }

        private void PhoneApplicationPage_BackKeyPress(
            object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            App.Quit();
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            txtPassword.Password = string.Empty;
        }

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            OpenDatabase();
        }

        private void imgWarning_ManipulationStarted(
            object sender, ManipulationStartedEventArgs e)
        {
            e.Complete();
            e.Handled = true;

            MessageBox.Show(AppResources.WarningStorePassword,
                (string)chkStore.Content, MessageBoxButton.OK);
        }

        private void txtPassword_KeyDown(
            object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdOpen.IsEnabled)
                OpenDatabase();
        }

        private void txtPassword_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            var hasPassword = txtPassword
                .Password.Length > 0;

            cmdOpen.IsEnabled = hasPassword;
            _cmdAppBarOpen.IsEnabled = hasPassword;
        }
    }
}