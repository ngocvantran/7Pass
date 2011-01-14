using System;
using System.Linq;
using Microsoft.Phone.Controls;

namespace KeePass.Utils
{
    public static class Navigation
    {
        private const string BASE_NS = "KeePass";

        public static void NavigateTo(
            this PhoneApplicationPage page,
            string url, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                url = string.Format(url, args
                    .Select(x => x.ToString())
                    .Select(Uri.EscapeDataString)
                    .ToArray());
            }

            page.NavigationService.Navigate(
                new Uri(url, UriKind.Relative));
        }

        public static void NavigateTo<T>(
            this PhoneApplicationPage page,
            string queries, params object[] args)
        {
            var path = typeof(T).FullName;
            path = path.Substring(BASE_NS.Length);
            path = path.Replace(".", "/") + ".xaml";

            if (!string.IsNullOrEmpty(queries))
                path += "?" + queries;

            NavigateTo(page, path, args);
        }

        public static void NavigateTo<T>(
            this PhoneApplicationPage page)
        {
            NavigateTo<T>(page, string.Empty);
        }
    }
}