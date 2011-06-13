using System;

namespace KeePass.Analytics
{
    internal class NullTrackImpl : ITrackImpl
    {
        public void Track(TrackingEvent info) {}
    }
}