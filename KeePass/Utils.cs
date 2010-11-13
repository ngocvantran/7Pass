using System;
using Microsoft.Phone.Controls;

namespace KeePass
{
    public static class Utils
    {
        public static void NavigateTo(
            this PhoneApplicationPage page, string url)
        {
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