using System;
using System.Net;
using System.Windows;

namespace KeePass.Sources.Web
{
    public partial class WebAuth
    {
        public WebAuth()
        {
            InitializeComponent();
        }

        public NetworkCredential GetCredentials()
        {
            if (!chkAuth.IsChecked == true)
                return null;

            return WebUtils.CreateCredentials(
                txtUser.Text, txtPassword.Password,
                txtDomain.Text);
        }

        private void chkAuth_Click(object sender, RoutedEventArgs e)
        {
            var visible = chkAuth.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            lblUser.Visibility = visible;
            txtUser.Visibility = visible;
            lblDomain.Visibility = visible;
            txtDomain.Visibility = visible;
            lblPassword.Visibility = visible;
            txtPassword.Visibility = visible;
        }
    }
}