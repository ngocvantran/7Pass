using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KeePass.Storage;
using KeePass.Utils;
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

            SetWorking(false);
            imgWarning.Source = ThemeData.GetImageSource("warning");
        }

        private void OpenDatabase()
        {
            SetWorking(true);

            var folder = NavigationContext.QueryString["db"];

            _wkOpen.RunWorkerAsync(new OpenArgs
            {
                Folder = folder,
                Dispatcher = Dispatcher,
                Password = txtPassword.Password,
                SavePassword = chkStore.IsChecked == true,
            });
        }

        private void SetWorking(bool working)
        {
            progBusy.IsLoading = working;
            ApplicationBar.IsVisible = !working;
            ContentPanel.IsHitTestVisible = !working;
        }

        private static void _wkOpen_DoWork(
            object sender, DoWorkEventArgs e)
        {
            var args = (OpenArgs)e.Argument;
            var database = new DatabaseInfo(args.Folder);

            e.Result = database.Open(args.Dispatcher,
                args.Password, args.SavePassword);
        }

        private void _wkOpen_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            SetWorking(false);

            if (e.Error != null)
            {
                var sendMail = MessageBox.Show(
                    Properties.Resources.ParseError,
                    Properties.Resources.PasswordTitle,
                    MessageBoxButton.OKCancel);

                if (sendMail == MessageBoxResult.OK)
                    ErrorReport.Report(e.Error);

                return;
            }

            switch ((OpenDbResults)e.Result)
            {
                case OpenDbResults.Success:
                    this.NavigateTo<GroupDetails>();
                    break;

                case OpenDbResults.IncorrectPassword:
                    MessageBox.Show(Properties.Resources.IncorrectPassword,
                        Properties.Resources.PasswordTitle,
                        MessageBoxButton.OK);
                    break;

                case OpenDbResults.CorruptedFile:
                    MessageBox.Show(Properties.Resources.CorruptedFile,
                        Properties.Resources.PasswordTitle,
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

            MessageBox.Show(Properties.Resources.WarningStorePassword,
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
            public string Folder { get; set; }
            public string Password { get; set; }
            public bool SavePassword { get; set; }
        }
    }
}