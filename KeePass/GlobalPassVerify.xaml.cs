using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using KeePass.Analytics;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class GlobalPassVerify
    {
        private readonly ApplicationBarIconButton _cmdOk;
        private readonly BackgroundWorker _wkVerify;

        public GlobalPassVerify()
        {
            InitializeComponent();

            _wkVerify = new BackgroundWorker();
            _wkVerify.DoWork += _wkVerify_DoWork;
            _wkVerify.RunWorkerCompleted +=
                _wkVerify_RunWorkerCompleted;

            _cmdOk = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AnalyticsTracker.Track("global_pass");
        }

        private void Verify()
        {
            txtPass.IsEnabled = false;
            ApplicationBar.IsVisible = false;

            _wkVerify.RunWorkerAsync(
                txtPass.Password);
        }

        private void KeePassPage_BackKeyPress(
            object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Quit();
        }

        private static void _wkVerify_DoWork(
            object sender, DoWorkEventArgs e)
        {
            var password = (string)e.Argument;
            var globalPass = GlobalPassHandler.Instance;

            if (globalPass.Verify(password))
            {
                e.Result = true;
                return;
            }

            e.Result = false;
            Thread.Sleep(1000);
        }

        private void _wkVerify_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            var correct = (bool)e.Result;
            if (correct)
            {
                NavigationService.GoBack();
                return;
            }

            txtPass.IsEnabled = true;
            ApplicationBar.IsVisible = true;

            txtPass.Focus();
            txtPass.SelectAll();
        }

        private void cmdClear_Click(
            object sender, EventArgs e)
        {
            txtPass.Password = string.Empty;
            txtPass.Focus();
        }

        private void cmdOK_Click(
            object sender, EventArgs e)
        {
            Verify();
        }

        private void txtPass_KeyDown(
            object sender, KeyEventArgs e)
        {
            if (e.IsEnter() && _cmdOk.IsEnabled)
                Verify();
        }

        private void txtPass_Loaded(
            object sender, RoutedEventArgs e)
        {
            txtPass.Focus();
        }

        private void txtPass_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            _cmdOk.IsEnabled = !string
                .IsNullOrEmpty(txtPass.Password);
        }
    }
}