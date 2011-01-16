using System;
using System.Windows.Controls;
using System.Windows.Input;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.Web
{
    public partial class WebDownload
    {
        private readonly ApplicationBarIconButton _cmdAppBarDownload;
        private readonly DownloadHandler _download;

        public WebDownload()
        {
            InitializeComponent();

            _download = new DownloadHandler(this);
            _cmdAppBarDownload = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];

            _download.Completed += _download_Completed;
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
            progList.IsLoading = working;
            cmdDownload.IsEnabled = !working;
        }

        private void _download_Completed(object sender, EventArgs e)
        {
            SetWorkState(false);
        }

        private void cmdDownload_Click(object sender, EventArgs e)
        {
            PerformDownload();
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter() && cmdDownload.IsEnabled)
                PerformDownload();
        }

        private void txtUrl_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            var isValidUrl = Uri.IsWellFormedUriString(
                txtUrl.Text, UriKind.Absolute);

            cmdDownload.IsEnabled = isValidUrl;
            _cmdAppBarDownload.IsEnabled = isValidUrl;
        }
    }
}