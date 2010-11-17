using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.Properties;

namespace KeePass
{
    public partial class Password
    {
        public Password()
        {
            InitializeComponent();
        }

        private void OpenDatabase()
        {
            AppSettingsService.Open(
                txtPassword.Password,
                chkStore.IsChecked == true);

            NavigationService.GoBack();
        }

        private void Image_ManipulationStarted(
            object sender, ManipulationStartedEventArgs e)
        {
            e.Complete();
            e.Handled = true;

            MessageBox.Show(AppResources
                .WarningStorePassword);
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

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            OpenDatabase();
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
            cmdOpen.IsEnabled = txtPassword
                .Password.Length > 0;
        }
    }
}