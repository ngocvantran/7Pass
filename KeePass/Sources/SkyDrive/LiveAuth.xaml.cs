using System;
using KeePass.Utils;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.SkyDrive
{
    public partial class LiveAuth
    {
        private readonly ProgressIndicator _indicator;

        public LiveAuth()
        {
            InitializeComponent();
            _indicator = AddIndicator();
        }

        private void CheckToken(Uri uri)
        {
            const string prefix = "?code=";

            var query = uri.Query;
            if (!query.StartsWith(prefix,
                StringComparison.InvariantCultureIgnoreCase))
                return;

            var code = query.Substring(prefix.Length);
            SkyDriveClient.GetToken(code, token =>
            {
                var folder = NavigationContext
                    .QueryString["folder"];

                this.NavigateTo<List>(
                    "token={0}&folder={1}",
                    token, folder);
            });
        }

        private void ShowLogin()
        {
            var theme = ThemeData.IsDarkTheme
                ? "Dark" : "Light";

            var url = string.Format(
                SkyDrive.Resources.AuthUrl,
                ApiKeys.SKYDRIVE_CLIENT_ID,
                ApiKeys.SKYDRIVE_REDIRECT, theme);

            browser.Navigate(new Uri(url));
        }

        private void browser_LoadCompleted(object sender,
            System.Windows.Navigation.NavigationEventArgs e)
        {
            _indicator.IsVisible = false;
        }

        private void browser_Loaded(object sender,
            System.Windows.RoutedEventArgs e)
        {
            browser.Navigate(new Uri(
                "http://login.live.com/logout.srf"));
        }

        private void browser_Navigating(
            object sender, NavigatingEventArgs e)
        {
            _indicator.IsVisible = true;

            try
            {
                var uri = e.Uri;
                if (uri.ToString().StartsWith(
                    ApiKeys.SKYDRIVE_REDIRECT))
                {
                    CheckToken(uri);
                    return;
                }

                if (uri.Host.Contains("live.com"))
                    return;

                e.Cancel = true;
                ShowLogin();
            }
            finally
            {
                _indicator.IsVisible = !e.Cancel;
            }
        }
    }
}