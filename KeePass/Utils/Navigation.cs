using System;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace KeePass.Utils
{
    public static class Navigation
    {
        private const string BASE_NS = "KeePass";

        /// <summary>
        /// Navigates back to databases page.
        /// </summary>
        /// <param name="page">The page.</param>
        public static void BackToDatabases(
            this PhoneApplicationPage page)
        {
            if (!GoBack<MainPage>(page))
                NavigateTo<MainPage>(page);
        }

        /// <summary>
        /// Navigates back to root group.
        /// </summary>
        /// <param name="page">The page.</param>
        public static void BackToRoot(
            this PhoneApplicationPage page)
        {
            if (!GoBack<GroupDetails>(page))
                NavigateTo<GroupDetails>(page);
        }

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
                    .Select(x => x.ToString())
                    .Select(Uri.EscapeDataString)
                    .ToArray());
            }

            return new Uri(url, UriKind.Relative);
        }

        public static bool GoBack<T>(
            this PhoneApplicationPage page)
            where T : PhoneApplicationPage
        {
            return GoBack(page, GetPathTo<T>());
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

        private static bool GoBack(Page page, Uri uri)
        {
            var service = page.NavigationService;
            if (!service.CanGoBack)
                return false;

            var backStack = service.BackStack
                .Select(x => x.Source)
                .ToList();

            var index = backStack.IndexOf(uri);
            if (index < 0)
                return false;

            for (var i = 0; i < index; i++)
                service.RemoveBackEntry();

            service.GoBack();
            return true;
        }
    }
}