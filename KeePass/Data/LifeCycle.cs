using System;
using System.Diagnostics;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Data
{
    internal static class LifeCycle
    {
        public static void Activated()
        {
            AppSettingsService.LoadSettings();
        }

        public static DatabaseCheckResults CheckDbState(
            PhoneApplicationPage page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            if (KeyCache.Database != null)
                return DatabaseCheckResults.Continue;

            page.NavigateTo(AppSettingsService.HasDatabase()
                ? "/Password.xaml" : "/Download.xaml");

            return DatabaseCheckResults.Terminate;
        }

        public static void Closing()
        {
            PreventEndDebug();
        }

        public static void Deactivated()
        {
            PreventEndDebug();
        }

        public static void Launching()
        {
            AppSettingsService.LoadSettings();
        }

        [Conditional("DEBUG")]
        private static void PreventEndDebug()
        {
            PhoneApplicationService.Current.State["Dummy"] = "Dummy";
        }
    }
}