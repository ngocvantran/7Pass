using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using KeePass.IO.Data;
using KeePass.IO.Utils;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Tasks;
using Resources = KeePass.Properties.Resources;

namespace KeePass.Sources.Web
{
    internal class DownloadHandler
    {
        private readonly string _folder;
        private readonly KeePassPage _page;

        /// <summary>
        /// Occurs when the database has been saved.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Occurs when web links are detected.
        /// </summary>
        public event EventHandler LinksDetected;

        public DownloadHandler(KeePassPage page, string folder)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (folder == null) throw new ArgumentNullException("folder");

            _page = page;
            _folder = folder;
        }

        public void Download(string url,
            ICredentials credentials)
        {
            WebUtils.Download(url, credentials,
                ResponseReady);
        }

        /// <summary>
        /// Raises the <see cref="Completed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnCompleted(EventArgs e)
        {
            if (Completed != null)
                Completed(this, e);
        }

        /// <summary>
        /// Raises the <see cref="LinksDetected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnLinksDetected(EventArgs e)
        {
            if (LinksDetected != null)
                LinksDetected(this, e);
        }

        private static bool DetectCertificateError(
            WebRequest request, WebException ex)
        {
            var uri = request.RequestUri;
            if (uri.Scheme.ToUpper() != "HTTPS")
                return false;

            var response = ex.Response as HttpWebResponse;
            if (response == null)
                return false;

            return response.StatusCode ==
                HttpStatusCode.NotFound;
        }

        private bool DetectLinks(
            Stream stream, WebRequest request)
        {
            var links = ExtractLinks(
                request.RequestUri, stream);

            if (links.Length == 0)
                return false;

            WebLinks.Links = links;
            WebLinks.Folder = _folder;
            WebLinks.Credentials = request.Credentials;

            OnLinksDetected(EventArgs.Empty);

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

            var regex = new Regex(Resources.LinkRegex,
                RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);

            return matches
                .Cast<Match>()
                .Select(x => x.Groups[2].Value)
                .Distinct()
                .Where(x => UrlUtils.IsValidUrl(baseUrl, x))
                .Select(x => new Uri(baseUrl, x))
                .Select(x => x.ToString())
                .Take(30)
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
                    var message = Resources.DownloadError +
                        ex.Message;

                    var certificate = DetectCertificateError(
                        request, ex);

                    if (!certificate)
                    {
                        dispatcher.BeginInvoke(() =>
                            MessageBox.Show(message,
                                Resources.DownloadTitle,
                                MessageBoxButton.OK));
                    }
                    else
                    {
                        message += Resources.InvalidCertificate;

                        dispatcher.BeginInvoke(() =>
                        {
                            var result = MessageBox.Show(message,
                                Resources.DownloadTitle,
                                MessageBoxButton.OKCancel);

                            if (result != MessageBoxResult.OK)
                                return;

                            new WebBrowserTask
                            {
                                Uri = new Uri(Resources
                                    .InvalidCertificateUrl),
                            }.Show();
                        });
                    }

                    return;
                }

                using (var stream = new MemoryStream())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        BufferEx.CopyStream(responseStream, stream);
                        stream.Position = 0;
                    }

                    if (string.IsNullOrEmpty(_folder))
                    {
                        VerifyAndSaveDb(request, dispatcher,
                            response, stream);
                    }
                    else
                    {
                        VerifyAndSaveKeyFile(request,
                            dispatcher, response, stream);
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
                Type = SourceTypes.Updatable,
                Source = DatabaseUpdater.WEB_UPDATER,
            });

            _page.Dispatcher.BeginInvoke(
                _page.BackToDBs);
        }

        private void SaveKeyFile(byte[] hash)
        {
            var info = new DatabaseInfo(_folder);

            info.SetKeyFile(hash);
            _page.Dispatcher.BeginInvoke(
                _page.BackToDBs);
        }

        private void VerifyAndSaveDb(HttpWebRequest request,
            Dispatcher dispatcher, HttpWebResponse response,
            MemoryStream stream)
        {
            var check = DatabaseVerifier
                .VerifyUnattened(stream);

            switch (check.Result)
            {
                case VerifyResultTypes.Warning:
                    dispatcher.BeginInvoke(() =>
                        MessageBox.Show(check.Message,
                            Resources.DownloadTitle,
                            MessageBoxButton.OK));

                    SaveDatabase(request, stream);
                    break;

                case VerifyResultTypes.Pass:
                    SaveDatabase(request, stream);
                    break;

                case VerifyResultTypes.Error:
                    // Not a database, try to extract links
                    if (response.StatusCode != HttpStatusCode.OK ||
                        !DetectLinks(stream, request))
                    {
                        dispatcher.BeginInvoke(() =>
                            MessageBox.Show(check.Message,
                                Resources.DownloadTitle,
                                MessageBoxButton.OK));
                    }
                    break;
            }
        }

        private void VerifyAndSaveKeyFile(
            HttpWebRequest request, Dispatcher dispatcher,
            HttpWebResponse response, MemoryStream stream)
        {
            var hash = KeyFile.GetKey(stream);
            if (hash != null)
            {
                SaveKeyFile(hash);
                return;
            }

            // Not a key file, try to extract links
            if (response.StatusCode != HttpStatusCode.OK ||
                !DetectLinks(stream, request))
            {
                dispatcher.BeginInvoke(() =>
                    MessageBox.Show(
                        Resources.InvalidKeyFile,
                        Resources.KeyFileTitle,
                        MessageBoxButton.OK));
            }
        }
    }
}