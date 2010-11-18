using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace KeePass
{
    public static class Utils
    {
        public static bool IsDarkTheme()
        {
            var v = (Visibility)Application.Current
                .Resources["PhoneLightThemeVisibility"];
            return v != Visibility.Visible;
        }

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

        public static void OpenHome(
            this PhoneApplicationPage page)
        {
            NavigateTo(page, "/MainPage.xaml");
        }

        public static void OpenSettings(
            this PhoneApplicationPage page)
        {
            NavigateTo(page, "/Settings.xaml");
        }
    }
}