using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using KeePass.Analytics;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class Password
    {
        private readonly ApplicationBarIconButton _cmdOpen;
        private readonly BackgroundWorker _wkOpen;

        private string _folder;
        private bool _hasKeyFile;

        public Password()
        {
            InitializeComponent();

            _wkOpen = new BackgroundWorker();
            _wkOpen.DoWork += _wkOpen_DoWork;
            _wkOpen.RunWorkerCompleted += _wkOpen_RunWorkerCompleted;

            _cmdOpen = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];

            imgWarning.Source = ThemeData.GetImageSource("warning");
            imgWarning.Visibility = GlobalPassHandler.Instance.HasGlobalPass
                ? Visibility.Collapsed : Visibility.Visible;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            _folder = NavigationContext.QueryString["db"];
            _hasKeyFile = new DatabaseInfo(_folder).HasKeyFile;

            UpdatePasswordStatus();
        }

        private void OpenDatabase()
        {
            progBusy.IsBusy = true;
            progBusy.Focus();

            var savePass = chkStore
                .IsChecked == true;

            _wkOpen.RunWorkerAsync(new OpenArgs
            {
                Folder = _folder,
                Dispatcher = Dispatcher,
                SavePassword = savePass,
                Password = txtPassword.Password,
            });

            AnalyticsTracker.Track(
                new TrackingEvent("open_db")
                {
                    {
                        "save_password", savePass
                            ? "true" : "false"
                        }
                });
        }

        private void UpdatePasswordStatus()
        {
            var hasPassword = _hasKeyFile ||
                txtPassword.Password.Length > 0;

            _cmdOpen.IsEnabled = hasPassword;
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
            progBusy.IsBusy = false;

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
                    txtPassword.Password = string.Empty;

                    string fromTile;
                    if (!NavigationContext.QueryString
                        .TryGetValue("fromTile", out fromTile))
                    {
                        this.NavigateTo<GroupDetails>();
                    }
                    else
                    {
                        this.NavigateTo<GroupDetails>(
                            "fromTile={0}", fromTile);
                    }

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
            if (!_cmdOpen.IsEnabled)
                return;

            if (e.IsEnter())
                OpenDatabase();
        }

        private void txtPassword_Loaded(
            object sender, RoutedEventArgs e)
        {
            txtPassword.Focus();
        }

        private void txtPassword_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            UpdatePasswordStatus();
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