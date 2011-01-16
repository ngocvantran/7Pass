using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;

namespace KeePass.Sources.Web
{
    public partial class WebBrowse
    {
        private readonly DownloadHandler _download;
        private readonly ObservableCollection<WebLinkInfo> _items;
        private NetworkCredential _credentials;

        public WebBrowse()
        {
            InitializeComponent();

            _download = new DownloadHandler(this);
            _items = new ObservableCollection<WebLinkInfo>();
            lstLinks.ItemsSource = _items;

            _download.Completed += _download_Completed;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var pars = NavigationContext.QueryString;

            _credentials = WebUtils.CreateCredentials(
                pars["user"], pars["password"], pars["domain"]);

            var links = pars["l"];
            ThreadPool.QueueUserWorkItem(
                _ => DisplayLinks(links));
        }

        private void DisplayLinks(string links)
        {
            var dispatcher = Dispatcher;
            var separated = links.Split(new[] {Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var link in separated)
            {
                var item = new WebLinkInfo(link);
                dispatcher.BeginInvoke(() =>
                    _items.Add(item));

                Thread.Sleep(50);
            }
        }

        private void _download_Completed(object sender, EventArgs e)
        {
            progList.IsLoading = false;
        }

        private void lstLinks_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var item = lstLinks.SelectedItem as WebLinkInfo;
            if (item == null)
                return;

            progList.IsLoading = true;
            lstLinks.SelectedItem = null;
            _download.Download(item.Url, _credentials);
        }
    }
}