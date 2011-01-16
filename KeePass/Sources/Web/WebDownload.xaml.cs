using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using KeePass.Data;
using KeePass.IO.Utils;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass.Sources.Web
{
    public partial class WebDownload
    {
        private readonly ObservableCollection<WebLinkInfo> _links;

        public WebDownload()
        {
            InitializeComponent();

            _links = new ObservableCollection<WebLinkInfo>();
            lstLinks.ItemsSource = _links;
        }

        private void DetectLinks(Dispatcher dispatcher,
            Stream stream, HttpWebRequest request)
        {
            var links = ExtractLinks(stream);

            var hasLinks = links.Count > 0;
            dispatcher.BeginInvoke(() => lblLinks.Visibility = hasLinks
                ? Visibility.Visible : Visibility.Collapsed);

            if (!hasLinks)
                return;

            var baseUrl = request.RequestUri;
            foreach (var link in links)
            {
                try
                {
                    var linkItem = new WebLinkInfo(
                        new Uri(baseUrl, link).ToString());

                    dispatcher.BeginInvoke(
                        () => _links.Add(linkItem));

                    Thread.Sleep(50);
                }
                catch {}
            }
        }

        private static IList<string> ExtractLinks(Stream stream)
        {
            var buffer = new byte[
                Math.Min(stream.Length, 1024 * 100)];

            stream.Position = 0;
            stream.Read(buffer, 0, buffer.Length);

            var html = Encoding.UTF8.GetString(
                buffer, 0, buffer.Length);

            var regex = new Regex("<a\\s+[^>]?href\\s?=\\s?(['\"])(.+?)\\1");
            var matches = regex.Matches(html);

            return matches
                .Cast<Match>()
                .Select(x => x.Groups[2].Value)
                .ToList();
        }

        private void PerformDownload()
        {
            _links.Clear();
            SetWorkState(true);

            WebUtils.Download(txtUrl.Text,
                ResponseReady);
        }

        private void ResponseReady(HttpWebRequest request,
            Func<HttpWebResponse> getResponse)
        {
            HttpWebResponse response;
            var dispatcher = Dispatcher;

            try
            {
                try
                {
                    response = getResponse();
                }
                catch (WebException ex)
                {
                    var message = Properties.Resources
                        .DownloadError + ex.Message;

                    dispatcher.BeginInvoke(() =>
                        MessageBox.Show(message,
                            Properties.Resources.DownloadTitle,
                            MessageBoxButton.OK));

                    return;
                }

                using (var stream = new MemoryStream())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        BufferEx.CopyStream(responseStream, stream);
                        stream.Position = 0;
                    }

                    if (SaveDatabase(dispatcher, request, stream))
                        return;

                    // Not a database, try to extract links
                    if (response.StatusCode == HttpStatusCode.OK)
                        DetectLinks(dispatcher, stream, request);
                }
            }
            finally
            {
                dispatcher.BeginInvoke(() =>
                    SetWorkState(false));
            }
        }

        private bool SaveDatabase(Dispatcher dispatcher,
            HttpWebRequest request, Stream stream)
        {
            if (!DatabaseVerifier.Verify(dispatcher, stream))
                return false;

            stream.Position = 0;
            var info = new DatabaseInfo();

            var url = request.RequestUri.ToString();
            info.SetDatabase(stream, new DatabaseDetails
            {
                Url = url,
                Source = WebUpdater.NAME,
                Name = Path.GetFileName(url),
            });

            dispatcher.BeginInvoke(GoBack<MainPage>);
            return true;
        }

        private void SetWorkState(bool working)
        {
            txtUrl.IsEnabled = !working;
            progList.IsLoading = working;
            cmdDownload.IsEnabled = !working;
        }

        private void cmdDownload_Click(object sender, RoutedEventArgs e)
        {
            PerformDownload();
        }

        private void lstLinks_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var item = lstLinks.SelectedItem as WebLinkInfo;
            if (item == null)
                return;

            txtUrl.Text = item.Url;
            txtUrl.SelectionStart = txtUrl.Text.Length;
            txtUrl.Focus();

            lstLinks.SelectedItem = null;
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter() && cmdDownload.IsEnabled)
                PerformDownload();
        }

        private void txtUrl_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            cmdDownload.IsEnabled = Uri.IsWellFormedUriString(
                txtUrl.Text, UriKind.Absolute);
        }
    }
}