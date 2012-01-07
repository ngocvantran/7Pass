using System;
using System.Windows;
using System.Windows.Input;
using KeePass.I18n;
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
            
            _cmdSet = AppButton(0);
            _cmdSet.Text = Strings.GlobalPass_Set;
            AppButton(1).Text = Strings.Clear;
        }

        private void SetPassword()
        {
            AppSettings.Instance.Password =
                BufferEx.GetHash(txtPass.Password);

            NavigationService.GoBack();
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            txtPass.Password = string.Empty;
            txtConfirm.Password = string.Empty;

            txtPass.Focus();
        }

        private void cmdSet_Click(object sender, EventArgs e)
        {
            SetPassword();
        }

        private void txtConfirm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsEnter())
                return;

            if (_cmdSet.IsEnabled)
                SetPassword();
        }

        private void txtConfirm_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            var password = txtConfirm.Password;

            _cmdSet.IsEnabled =
                !string.IsNullOrEmpty(password) &&
                    txtPass.Password == password;
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsEnter())
                return;

            txtConfirm.Focus();
            txtConfirm.SelectAll();
        }

        private void txtPass_Loaded(object sender, RoutedEventArgs e)
        {
            txtPass.Focus();
        }
    }
}
