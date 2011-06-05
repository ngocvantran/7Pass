using System;
using System.Linq;
using Microsoft.Phone.Controls;

namespace KeePass.Utils
{
    public static class Navigation
    {
        private const string BASE_NS = "KeePass";

        public static Uri GetPathTo<T>()
            where T : PhoneApplicationPage
        {
            return GetPathTo<T>(string.Empty);
        }

        public static Uri GetPathTo<T>(
            string queries, params object[] args)
            where T : PhoneApplicationPage
        {
            var path = typeof(T).FullName;
            path = path.Substring(BASE_NS.Length);
            path = path.Replace(".", "/") + ".xaml";

            if (!string.IsNullOrEmpty(queries))
                path += "?" + queries;

            return GetPathTo(path, args);
        }

        public static Uri GetPathTo(
            string url, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                url = string.Format(url, args
                    .Select(x => x != null
                        ? x.ToString()
                        : string.Empty)
                    .Select(Uri.EscapeDataString)
                    .ToArray());
            }

            return new Uri(url, UriKind.Relative);
        }

        public static void NavigateTo(
            this PhoneApplicationPage page,
            string url, params object[] args)
        {
            var uri = GetPathTo(url, args);
            page.NavigationService.Navigate(uri);
        }

        public static void NavigateTo<T>(
            this PhoneApplicationPage page,
            string queries, params object[] args)
            where T : PhoneApplicationPage
        {
            var uri = GetPathTo<T>(queries, args);
            page.NavigationService.Navigate(uri);
        }

        public static void NavigateTo<T>(
            this PhoneApplicationPage page)
            where T : PhoneApplicationPage
        {
            var uri = GetPathTo<T>();
            page.NavigationService.Navigate(uri);
        }
    }
}