using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KeePass.Data;
using KeePass.Properties;
using KeePass.Services;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class Password
    {
        private readonly ApplicationBarIconButton _cmdAppBarOpen;
        private readonly BackgroundWorker _wkOpen;

        public Password()
        {
            InitializeComponent();

            _wkOpen = new BackgroundWorker();
            _wkOpen.DoWork += _wkOpen_DoWork;
            _wkOpen.RunWorkerCompleted += _wkOpen_RunWorkerCompleted;

            _cmdAppBarOpen = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];

            if (Theme.IsDarkTheme())
                return;

            var uri = new Uri("/Images/warning.light.png",
                UriKind.Relative);

            SetWorking(false);
            imgWarning.Source = new BitmapImage(uri);
        }

        private void OpenDatabase()
        {
            SetWorking(true);

            _wkOpen.RunWorkerAsync(new OpenArgs
            {
                Dispatcher = Dispatcher,
                Password = txtPassword.Password,
                SavePassword = chkStore.IsChecked == true,
            });
        }

        private void SetWorking(bool working)
        {
            ApplicationBar.IsVisible = !working;
            ContentPanel.IsHitTestVisible = !working;

            progBusy.Visibility = working
                ? Visibility.Visible
                : Visibility.Collapsed;
            progBusy.IsIndeterminate = working;
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

        private static void _wkOpen_DoWork(
            object sender, DoWorkEventArgs e)
        {
            var args = (OpenArgs)e.Argument;
            e.Result = AppSettingsService.Open(args.Password,
                args.SavePassword, args.Dispatcher);
        }

        private void _wkOpen_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            SetWorking(false);

            if (e.Error != null)
            {
                var sendMail = MessageBox.Show(
                    AppResources.ParseError,
                    AppResources.PasswordTitle,
                    MessageBoxButton.OKCancel);

                if (sendMail == MessageBoxResult.OK)
                    ErrorReport.Report(e.Error);

                return;
            }

            switch ((OpenDbResults)e.Result)
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

        private class OpenArgs
        {
            public Dispatcher Dispatcher { get; set; }
            public string Password { get; set; }
            public bool SavePassword { get; set; }
        }
    }
}