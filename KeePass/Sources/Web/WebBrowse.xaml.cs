using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;

namespace KeePass.Sources.Web
{
    public partial class WebBrowse
    {
        private readonly ObservableCollection<WebLinkInfo> _items;
        private DownloadHandler _download;

        public WebBrowse()
        {
            InitializeComponent();

            _items = new ObservableCollection<WebLinkInfo>();
            lstLinks.ItemsSource = _items;
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
            var dispatcher = Dispatcher;

            dispatcher.BeginInvoke(
                () => _items.Clear());

            foreach (var link in links)
            {
                var item = new WebLinkInfo(link);
                dispatcher.BeginInvoke(() =>
                    _items.Add(item));

                Thread.Sleep(50);
            }
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

        private void lstLinks_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var item = lstLinks.SelectedItem as WebLinkInfo;
            if (item == null)
                return;

            progBusy.IsBusy = true;
            lstLinks.IsEnabled = false;
            lstLinks.SelectedItem = null;

            _download.Download(item.Url,
                WebLinks.Credentials);
        }
    }
}