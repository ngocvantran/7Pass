using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using KeePass.Utils;

namespace KeePass.Sources.Web
{
    public partial class WebAuth
    {
        public event EventHandler Completed;

        public bool UseAuth
        {
            get { return chkAuth.IsChecked == true; }
            set { chkAuth.IsChecked = value; }
        }

        public WebAuth()
        {
            InitializeComponent();
        }

        public NetworkCredential GetCredentials()
        {
            if (!UseAuth)
                return null;

            return WebUtils.CreateCredentials(
                txtUser.Text, txtPassword.Password,
                txtDomain.Text);
        }

        /// <summary>
        /// Raises the <see cref="Completed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnCompleted(EventArgs e)
        {
            if (Completed != null)
                Completed(this, e);
        }

        private void chkAuth_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var visible = UseAuth
                ? Visibility.Visible
                : Visibility.Collapsed;

            lblUser.Visibility = visible;
            txtUser.Visibility = visible;
            lblDomain.Visibility = visible;
            txtDomain.Visibility = visible;
            lblPassword.Visibility = visible;
            txtPassword.Visibility = visible;
        }

        private void txtDomain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter())
                OnCompleted(EventArgs.Empty);
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter())
                txtDomain.Focus();
        }

        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter())
                txtPassword.Focus();
        }
    }
}