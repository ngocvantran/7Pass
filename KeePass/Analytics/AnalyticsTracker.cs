using System;
using KeePass.Utils;

namespace KeePass.Analytics
{
    internal static class AnalyticsTracker
    {
        private static ITrackImpl _impl;

        static AnalyticsTracker()
        {
            var allow = AppSettings
                .Instance.AllowAnalytics;

            if (allow != null && allow.Value)
                Collect();
            else
                Disable();
        }

        public static void Collect()
        {
            _impl = new MixPanelTrack();
        }

        public static void Disable()
        {
            _impl = new NullTrackImpl();
        }

        public static void Track(string eventName)
        {
            Track(new TrackingEvent(eventName));
        }

        public static void Track(TrackingEvent info)
        {
            _impl.Track(info);
        }
    }
}