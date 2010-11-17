using System;
using Microsoft.Phone.Controls;

namespace KeePass
{
    public static class Utils
    {
        public static void NavigateTo(
            this PhoneApplicationPage page,
            string url, params object[] args)
        {
            if (args != null && args.Length > 0)
                url = string.Format(url, args);

            page.NavigationService.Navigate(
                new Uri(url, UriKind.Relative));
        }

        public static void OpenDownload(
            this PhoneApplicationPage page)
        {
            NavigateTo(page, "/Download.xaml");
        }

        public static void OpenSettings(
            this PhoneApplicationPage page)
        {
            NavigateTo(page, "/Settings.xaml");
        }
    }
}