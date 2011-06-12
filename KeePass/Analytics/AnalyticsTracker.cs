using System;
using System.Net;
using System.Threading;

namespace KeePass.Analytics
{
    internal static class AnalyticsTracker
    {
#if DEBUG
        private const string URI = "http://api.mixpanel.com/track?test=1&data=";
#else
        private const string URI = "http://api.mixpanel.com/track?data=";
#endif

        private static WebClient _client;

        public static void Track(string eventName)
        {
            Track(new TrackingEvent(eventName));
        }

        public static void Track(TrackingEvent info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var json = info.ToJson();
            ThreadPool.QueueUserWorkItem(
                x => Send((string)x), json);
        }

        private static void Send(string json)
        {
            if (_client == null)
            {
                _client = new WebClient();
                _client.DownloadStringCompleted +=
                    _client_DownloadStringCompleted;
            }

            var uri = new Uri(URI + json);
            _client.DownloadStringAsync(uri);
        }

        private static void _client_DownloadStringCompleted(
            object sender, DownloadStringCompletedEventArgs e) {}
    }
}