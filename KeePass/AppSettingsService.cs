using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

namespace KeePass
{
    public static class AppSettingsService
    {
        private const string KEY = "Settings.xml";

        public static void Activated()
        {
            LoadSettings();
        }

        public static void Closing()
        {
            SaveSettings();
        }

        public static void Deactivated()
        {
            SaveSettings();
        }

        public static void Launching()
        {
            LoadSettings();
        }

        private static void LoadSettings()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (!store.FileExists(KEY))
                    return;

                using (var stream = store.OpenFile(
                    KEY, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new DataContractSerializer(
                        typeof(AppSettings));

                    var settings = (AppSettings)
                        serializer.ReadObject(stream);

                    KeyCache.Password = settings.Password;
                    KeyCache.StorePassword = settings.StorePassword;
                }
            }
        }

        private static void SaveSettings()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                using (var stream = store.CreateFile(KEY))
                {
                    var serializer = new DataContractSerializer(
                        typeof(AppSettings));

                    var settings = new AppSettings
                    {
                        StorePassword = KeyCache.StorePassword,
                    };

                    if (KeyCache.StorePassword)
                        settings.Password = KeyCache.Password;

                    serializer.WriteObject(stream, settings);
                }
            }
        }
    }
}