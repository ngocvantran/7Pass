using System;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass.Sources
{
    internal static class SourceCapabilityUpdater
    {
        private const string KEY = "DbVersion";
        private const string VALUE = "3.1";

        public static void Update()
        {
            var instance = AppSettings.Instance;

            var version = instance[KEY];
            if (version == null || version != VALUE)
                return;

            var databases = DatabaseInfo.GetAll();

            foreach (var db in databases)
            {
                var details = db.Details;
                if (details.Source != DatabaseUpdater.DROPBOX_UPDATER)
                    continue;

                if (details.Type == SourceTypes.Synchronizable)
                    continue;

                details.Type = SourceTypes.Synchronizable;
                db.SaveDetails();
            }

            instance[KEY] = VALUE;
        }
    }
}