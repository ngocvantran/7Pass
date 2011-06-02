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
            _download.NavigationFailed += _download_NavigationFailed;
        }

        private void PerformDownload()
        {
            SetWorkState(true);

            _download.Download(txtUrl.Text,
                ucAuth.GetCredentials());
        }

        private void SetWorkState(bool working)
        {
            txtUrl.IsEnabled = !working;
            ucAuth.IsEnabled = !working;
            progList.IsLoading = working;
            ApplicationBar.IsVisible = !working;
        }

        private void _download_Completed(object sender, EventArgs e)
        {
            SetWorkState(false);
        }

        private void _download_NavigationFailed(object sender, EventArgs e)
        {
            SetWorkState(false);
        }

        private void cmdDownload_Click(object sender, EventArgs e)
        {
            PerformDownload();
        }

        private void mnuBoxNet_Click(object sender, EventArgs e)
        {
            ucAuth.UseAuth = true;
            txtUrl.Text = "https://www.box.net/dav";
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
            var isValidUrl = Uri.IsWellFormedUriString(
                txtUrl.Text, UriKind.Absolute);

            _cmdDownload.IsEnabled = isValidUrl;
        }

        private void ucAuth_Completed(object sender, EventArgs e)
        {
            PerformDownload();
        }
    }
}