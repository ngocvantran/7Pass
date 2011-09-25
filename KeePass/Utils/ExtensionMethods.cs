using System;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Windows.Input;
using Microsoft.Phone.Controls;

namespace KeePass.Utils
{
    internal static class ExtensionMethods
    {
        public static void DeleteDirectory(
            this IsolatedStorageFile store,
            string path, bool recursive)
        {
            if (recursive)
            {
                var basePath = path + "/";

                var folders = store.GetDirectoryNames(basePath);
                foreach (var folder in folders)
                {
                    DeleteDirectory(store,
                        basePath + folder, true);
                }

                var files = store.GetFileNames(basePath + "*");
                foreach (var file in files)
                    store.DeleteFile(basePath + file);
            }

            store.DeleteDirectory(path);
        }

        public static string GetRelativeTime(
            this string dateValue, string format)
        {
            DateTime date;
            var parsed = DateTime.TryParseExact(dateValue,
                format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date);

            if (!parsed)
                return string.Empty;

            date = date.ToLocalTime();
            return (string)new RelativeTimeConverter()
                .Convert(date, typeof(string),
                    null, CultureInfo.CurrentUICulture);
        }

        public static bool IsEnter(this KeyEventArgs e)
        {
            return e.Key == Key.Enter ||
                e.PlatformKeyCode == 0x0A;
        }

        public static string RemoveKdbx(this string title)
        {
            const string extention = ".kdbx";

            if (title.EndsWith(extention, StringComparison
                .InvariantCultureIgnoreCase))
            {
                title = title.Substring(0,
                    title.Length - extention.Length);
            }

            return title;
        }
    }
}