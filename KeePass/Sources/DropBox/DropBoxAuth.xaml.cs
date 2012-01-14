using System;
using System.Net;
using System.Windows;
using DropNet;
using KeePass.Utils;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.DropBox
{
    public partial class DropBoxAuth
    {
        private const string CALL_BACK = "http://7pass.wordpress.com";

        private readonly DropNetClient _client;
        private readonly ProgressIndicator _indicator;

        public DropBoxAuth()
        {
            InitializeComponent();

            _indicator = AddIndicator();
            _client = DropBoxUtils.Create();
        }

        private void CheckToken()
        {
            _client.GetAccessTokenAsync(x =>
            {
                var folder = NavigationContext
                    .QueryString["folder"];

                this.NavigateTo<List>(
                    "token={0}&secret={1}&folder={2}",
                    x.Token, x.Secret, folder);
            }, ex => ShowError());
        }

        private void ShowError()
        {
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(
                    DropBoxResources.GetTokenError,
                    "DropBox",
                    MessageBoxButton.OK));
        }

        private void browser_LoadCompleted(object sender,
            System.Windows.Navigation.NavigationEventArgs e)
        {
            _indicator.IsVisible = false;
        }

        private void browser_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Network.CheckNetwork())
                return;

            _client.GetTokenAsync(x =>
            {
                var url = _client.BuildAuthorizeUrl(x, CALL_BACK);

                url = "https://www.dropbox.com/logout?cont=" +
                    HttpUtility.UrlEncode(url);

                Dispatcher.BeginInvoke(() =>
                    browser.Navigate(new Uri(url)));
            }, ex => ShowError());
        }

        private void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            _indicator.IsVisible = !e.Cancel;

            if (e.Uri.ToString().StartsWith(CALL_BACK))
                CheckToken();
        }
    }
}