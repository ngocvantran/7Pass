using System;
using KeePass.Data;
using KeePass.IO;
using Microsoft.Phone.Controls;

namespace KeePass
{
    public static class Navigation
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

        public static void NavigateTo(
            this PhoneApplicationPage page,
            Group group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            NavigateTo(page,
                "/MainPage.xaml?id={0}",
                group.ID);
        }

        public static void NavigateTo(
            this PhoneApplicationPage page,
            Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            NavigateTo(page,
                "/EntryPage.xaml?id={0}",
                entry.ID);
        }

        public static void NavigateTo(
            this PhoneApplicationPage page,
            DatabaseItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            var source = item.Source;

            var group = source as Group;
            if (group != null)
            {
                NavigateTo(page, group);
                return;
            }

            NavigateTo(page, (Entry)source);
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

        public static void OpenSearch(
            this PhoneApplicationPage page)
        {
            NavigateTo(page, "/Search.xaml");
        }

        public static void OpenSettings(
            this PhoneApplicationPage page)
        {
            NavigateTo(page, "/Settings.xaml");
        }
    }
}