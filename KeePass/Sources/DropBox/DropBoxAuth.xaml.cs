using System;
using System.Net;
using System.Windows;
using DropNet;
using KeePass.Utils;
using Microsoft.Phone.Controls;

namespace KeePass.Sources.DropBox
{
    public partial class DropBoxAuth
    {
        private const string CALL_BACK = "http://7pass.wordpress.com";
        private readonly DropNetClient _client;

        public DropBoxAuth()
        {
            InitializeComponent();

            _client = DropBoxInfo.Create();
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
            }, ex => { throw ex; });
        }

        private void browser_Loaded(object sender, RoutedEventArgs e)
        {
            _client.GetTokenAsync(x =>
            {
                var url = _client.BuildAuthorizeUrl(x, CALL_BACK);

                url = "https://www.dropbox.com/logout?cont=" +
                    HttpUtility.UrlEncode(url);

                Dispatcher.BeginInvoke(() =>
                    browser.Navigate(new Uri(url)));
            }, ex => { throw ex; });
        }

        private void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(CALL_BACK))
                CheckToken();
        }
    }
}