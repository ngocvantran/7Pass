using System;

namespace KeePass.Analytics
{
    internal interface ITrackImpl
    {
        void Track(TrackingEvent info);
    }
}