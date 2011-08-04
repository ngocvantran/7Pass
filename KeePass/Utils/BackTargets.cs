using System;
using System.Linq;
using Microsoft.Phone.Controls;

namespace KeePass.Utils
{
    internal static class BackTargets
    {
        public static void BackToDBs(
            this PhoneApplicationPage page)
        {
            page.BackTo<MainPage>();
        }

        public static void BackToRoot(
            this PhoneApplicationPage page)
        {
            page.BackTo<GroupDetails>();
        }

        public static void Quit(
            this PhoneApplicationPage page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            var service = page.NavigationService;

            while (service.BackStack.Any())
                service.RemoveBackEntry();

            service.GoBack();
        }
    }
}