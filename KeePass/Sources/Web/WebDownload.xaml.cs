using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.Web
{
    public partial class WebDownload
    {
        private readonly ApplicationBarIconButton _cmdDownload;
        private DownloadHandler _download;

        public WebDownload()
        {
            InitializeComponent();

            _cmdDownload = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ucAuth.ClearPassword();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            _download = new DownloadHandler(this,
                NavigationContext.QueryString["folder"]);

            _download.Completed += _download_Completed;
            _download.LinksDetected += _download_LinksDetected;

            LoadQueries();
        }

        private void LoadQueries()
        {
            string value;
            var queries = NavigationContext.QueryString;

            if (!queries.TryGetValue("url", out value))
                return;

            txtUrl.Text = value;

            if (!queries.TryGetValue("user", out value))
                return;

            ucAuth.UseAuth = true;
            ucAuth.User = value;

            if (!queries.TryGetValue("pass", out value))
                return;

            ucAuth.Password = value;

            PerformDownload();
        }

        private void PerformDownload()
        {
            prgBusy.IsBusy = true;

            _download.Download(txtUrl.Text,
                ucAuth.GetCredentials());
        }

        private void _download_Completed(object sender, EventArgs e)
        {
            prgBusy.IsBusy = false;
        }

        private void _download_LinksDetected(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(
                this.NavigateTo<WebBrowse>);
        }

        private void cmdDownload_Click(object sender, EventArgs e)
        {
            PerformDownload();
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter() && _cmdDownload.IsEnabled)
                PerformDownload();
        }

        private void txtUrl_Loaded(object sender, RoutedEventArgs e)
        {
            txtUrl.Focus();
            txtUrl.SelectionStart = txtUrl.Text.Length;
        }

        private void txtUrl_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            var isValidUrl = WebUtils
                .IsValidUrl(txtUrl.Text);

            _cmdDownload.IsEnabled = isValidUrl;
        }

        private void ucAuth_Completed(object sender, EventArgs e)
        {
            PerformDownload();
        }
    }
}