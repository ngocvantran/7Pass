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
            OpenDbResults result;
            try
            {
                result = AppSettingsService.Open(
                    txtPassword.Password,
                    chkStore.IsChecked == true);
            }
            catch (Exception ex)
            {
                var sendMail = MessageBox.Show(
                    AppResources.ParseError,
                    AppResources.PasswordTitle,
                    MessageBoxButton.OKCancel);

                if (sendMail == MessageBoxResult.OK)
                    ErrorReport.Report(ex);

                return;
            }

            switch (result)
            {
                case OpenDbResults.Success:
                    NavigationService.GoBack();
                    break;

                case OpenDbResults.IncorrectPassword:
                    MessageBox.Show(AppResources.IncorrectPassword,
                        AppResources.PasswordTitle,
                        MessageBoxButton.OK);
                    break;

                case OpenDbResults.CorruptedFile:
                    MessageBox.Show(AppResources.CorruptedFile,
                        AppResources.PasswordTitle,
                        MessageBoxButton.OK);
                    break;
            }
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
            if (!cmdOpen.IsEnabled)
                return;

            if (e.Key == Key.Enter || e.PlatformKeyCode == 0x0A)
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