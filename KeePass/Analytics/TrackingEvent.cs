using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeePass.IO;
using KeePass.Utils;
using Newtonsoft.Json.Linq;

namespace KeePass.Analytics
{
    internal class TrackingEvent : Dictionary<string, string>
    {
        
        private static readonly string _source;
        private readonly string _eventName;

        static TrackingEvent()
        {
#if FREEWARE
            _source = "7Pass_Free";
#else
            _source = "7Pass";

            if (TrialManager.IsTrial)
                _source += "_Trial";
#endif
        }

        public TrackingEvent(string eventName)
        {
            if (eventName == null)
                throw new ArgumentNullException("eventName");

            _eventName = eventName;
        }

        public string ToJson()
        {
            var values = new Dictionary
                <string, string>(this);

            values.AddOrSet("mp_source", _source);
            values.AddOrSet("token", TrackInfo.TOKEN);

            var settings = AppSettings.Instance;
            values.AddOrSet("distinct_id",
                settings.InstanceID);

            var properties = values.Select(x =>
                new JProperty(x.Key, x.Value))
                .ToArray();

            var json = new JObject(
                new JProperty("event", _eventName),
                new JProperty("properties",
                    new JObject(properties)));

            return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(json.ToString()));
        }
    }
}