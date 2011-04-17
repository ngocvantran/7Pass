using System;
using System.Windows;
using KeePass.IO.Utils;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class GlobalPass
    {
        private readonly ApplicationBarIconButton _cmdSet;

        public GlobalPass()
        {
            InitializeComponent();

            _cmdSet = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            txtPass.Password = string.Empty;
            txtConfirm.Password = string.Empty;

            txtPass.Focus();
        }

        private void cmdSet_Click(object sender, EventArgs e)
        {
            AppSettings.Instance.Password =
                BufferEx.GetHash(txtPass.Password);

            NavigationService.GoBack();
        }

        private void txtConfirm_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            var password = txtConfirm.Password;

            _cmdSet.IsEnabled =
                !string.IsNullOrEmpty(password) &&
                    txtPass.Password == password;
        }

        private void txtPass_Loaded(object sender, RoutedEventArgs e)
        {
            txtPass.Focus();
        }
    }
}