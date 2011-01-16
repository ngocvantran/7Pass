using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using KeePass.IO.Utils;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass.Sources.Web
{
    internal class DownloadHandler
    {
        private readonly KeePassPage _page;

        public event EventHandler Completed;

        public DownloadHandler(KeePassPage page)
        {
            if (page == null)
                throw new ArgumentNullException("page");
            _page = page;
        }

        public void Download(string url, ICredentials credentials)
        {
            WebUtils.Download(url, credentials, ResponseReady);
        }

        /// <summary>
        /// Raises the <see cref="Completed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnCompleted(EventArgs e)
        {
            if (Completed != null)
                Completed(this, e);
        }

        private bool DetectLinks(
            Stream stream, WebRequest request)
        {
            var links = ExtractLinks(
                request.RequestUri, stream);

            if (links.Length == 0)
                return false;

            var sb = new StringBuilder();
            foreach (var link in links)
                sb.AppendLine(link);

            var credentials = request.Credentials as NetworkCredential
                ?? new NetworkCredential();

            _page.Dispatcher.BeginInvoke(() => _page.NavigateTo<WebBrowse>(
                "l={0}&user={1}&password={2}&domain={3}",
                sb.ToString(), credentials.UserName,
                credentials.Password, credentials.Domain));

            return true;
        }

        private static string[] ExtractLinks(
            Uri baseUrl, Stream stream)
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
                .Select(x => new Uri(baseUrl, x))
                .Select(x => x.ToString())
                .ToArray();
        }

        private void ResponseReady(HttpWebRequest request,
            Func<HttpWebResponse> getResponse)
        {
            HttpWebResponse response;
            var dispatcher = _page.Dispatcher;

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

                    var error = DatabaseVerifier.VerifyUnattened(stream);

                    if (error == null)
                    {
                        SaveDatabase(request, stream);
                        return;
                    }

                    // Not a database, try to extract links
                    if (response.StatusCode != HttpStatusCode.OK ||
                        !DetectLinks(stream, request))
                    {
                        dispatcher.BeginInvoke(() =>
                            MessageBox.Show(error,
                                Properties.Resources.DownloadTitle,
                                MessageBoxButton.OK));
                    }
                }
            }
            finally
            {
                dispatcher.BeginInvoke(() =>
                    OnCompleted(EventArgs.Empty));
            }
        }

        private void SaveDatabase(
            WebRequest request, Stream stream)
        {
            stream.Position = 0;
            var info = new DatabaseInfo();

            var name = Path.GetFileName(
                request.RequestUri.ToString());
            var url = WebUtils.Serialize(request);

            info.SetDatabase(stream, new DatabaseDetails
            {
                Url = url,
                Name = name,
                Source = DatabaseUpdater.WEB_UPDATER,
            });

            _page.Dispatcher.BeginInvoke(
                _page.GoBack<MainPage>);
        }
    }
}