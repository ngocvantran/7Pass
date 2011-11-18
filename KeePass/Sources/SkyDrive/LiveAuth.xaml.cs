using System;
using System.Linq;
using System.Windows.Navigation;
using KeePass.Utils;
using Microsoft.Phone.Controls;

namespace KeePass.Sources.SkyDrive
{
    public partial class LiveAuth
    {
        private bool _attemptLogout;

        public LiveAuth()
        {
            InitializeComponent();
        }

        private void CheckToken(Uri uri)
        {
            var parts = uri.Fragment
                .Substring(1)
                .Split('&')
                .Select(x => x.Split('='))
                .ToDictionary(x => x[0], x => x[1]);

            string token;
            if (parts.TryGetValue("access_token", out token))
                this.NavigateTo<List>("token={0}", token);
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            _attemptLogout = e.NavigationMode ==
                NavigationMode.Back;
        }

        private void ShowLogin()
        {
            var theme = ThemeData.IsDarkTheme
                ? "Dark" : "Light";
            var url = string.Format(
                SkyDrive.Resources.AuthUrl,
                SkyDriveInfo.CLIENT_ID,
                SkyDriveInfo.REDIRECT, theme);

            browser.Navigate(new Uri(url));
        }

        private void browser_Loaded(object sender,
            System.Windows.RoutedEventArgs e)
        {
            if (!_attemptLogout)
                ShowLogin();
            else
            {
                browser.Navigate(new Uri(
                    "http://login.live.com/logout.srf"));
            }
        }

        private void browser_Navigating(
            object sender, NavigatingEventArgs e)
        {
            var uri = e.Uri;
            if (uri.ToString().StartsWith(SkyDriveInfo.REDIRECT))
            {
                e.Cancel = true;
                CheckToken(uri);

                return;
            }

            if (!_attemptLogout || uri.Host.Contains("live.com"))
                return;

            e.Cancel = true;
            ShowLogin();
        }
    }
}