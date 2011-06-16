using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using KeePass.Data;

namespace KeePass.Sources.Web
{
    public partial class WebBrowse
    {
        private DownloadHandler _download;

        public WebBrowse()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            if (!WebLinks.HasData)
            {
                NavigationService.GoBack();
                return;
            }

            _download = new DownloadHandler(
                this, WebLinks.Folder);
            _download.Completed += _download_Completed;
            _download.LinksDetected += _download_LinksDetected;

            ThreadPool.QueueUserWorkItem(
                _ => DisplayLinks(WebLinks.Links));
        }

        private void DisplayLinks(
            IEnumerable<string> links)
        {
            var items = links
                .Select(x => new WebLinkInfo(x))
                .ToList();

            lstLinks.SetItems(items);
        }

        private void _download_Completed(object sender, EventArgs e)
        {
            progBusy.IsBusy = false;
            lstLinks.IsEnabled = true;
        }

        private void _download_LinksDetected(
            object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(
                _ => DisplayLinks(WebLinks.Links));
        }

        private void lstLinks_Navigation(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var item = e.Item as WebLinkInfo;
            if (item == null)
                return;

            progBusy.IsBusy = true;
            lstLinks.IsEnabled = false;

            _download.Download(item.Url,
                WebLinks.Credentials);
        }
    }
}