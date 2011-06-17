using System;
using System.IO;
using System.Net;
using System.Threading;

namespace KeePass.Analytics
{
    internal class MixPanelTrack : ITrackImpl
    {
#if DEBUG
        private const string URI = "http://api.mixpanel.com/track?test=1&data=";
#else
        private const string URI = "http://api.mixpanel.com/track?data=";
#endif

        public void Track(TrackingEvent info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var json = info.ToJson();
            ThreadPool.QueueUserWorkItem(
                x => Send((string)x), json);
        }

        private static void Send(string json)
        {
            var uri = new Uri(URI + json);
            var request = WebRequest.Create(uri);

            request.BeginGetResponse(
                ResponseReady, request);
        }

        private static void ResponseReady(IAsyncResult ar)
        {
#if DEBUG
            var request = (WebRequest)ar.AsyncState;
            var response = request.EndGetResponse(ar);

            var content = new StreamReader(
                response.GetResponseStream())
                .ReadToEnd();
#endif
        }
    }
}