using System;
using System.Windows;
using System.Windows.Input;
using DropNet;
using DropNet.Exceptions;
using DropNet.Models;
using KeePass.I18n;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.DropBox
{
    public partial class DropBox
    {
        private readonly ApplicationBarIconButton _cmdOpen;

        public DropBox()
        {
            InitializeComponent();

            _cmdOpen = AppButton(0);
            _cmdOpen.Text = Strings.DropBox_Login;
        }

        private void LoginCompleted(UserLogin info)
        {
            Dispatcher.BeginInvoke(() =>
            {
                progBusy.IsBusy = false;

                txtPassword.Password = string.Empty;

                var folder = NavigationContext
                    .QueryString["folder"];

                this.NavigateTo<List>(
                    "token={0}&secret={1}&path=/&folder={2}",
                    info.Token, info.Secret, folder);
            });
        }

        private void LoginFailed(DropboxException ex)
        {
            Dispatcher.BeginInvoke(() =>
            {
                progBusy.IsBusy = false;

                MessageBox.Show(DropBoxResources.LoginFailure,
                    DropBoxResources.LoginTitle,
                    MessageBoxButton.OK);

                txtEmail.Focus();
                txtEmail.SelectAll();
            });
        }

        private void PerformLogin()
        {
            if (!Network.CheckNetwork())
                return;

            progBusy.IsBusy = true;

            var client = new DropNetClient(
                DropBoxInfo.KEY, DropBoxInfo.SECRET);

            client.LoginAsync(txtEmail.Text, txtPassword.Password,
                LoginCompleted, LoginFailed);
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            PerformLogin();
        }

        private void txtEmail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter())
                txtPassword.Focus();
        }

        private void txtEmail_Loaded(object sender, RoutedEventArgs e)
        {
            txtEmail.Focus();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter())
                PerformLogin();
        }

        private void txt_Changed(object sender, EventArgs e)
        {
            var hasData = txtEmail.Text.Length > 0 &&
                txtPassword.Password.Length > 0;

            _cmdOpen.IsEnabled = hasData;
        }
    }
}
