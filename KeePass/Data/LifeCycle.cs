using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Data
{
    internal static class LifeCycle
    {
        public static void Activated()
        {
            LoadState();
        }

        public static DatabaseCheckResults CheckDbState(
            PhoneApplicationPage page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            if (KeyCache.Database != null)
            {
                var aware = page as ILifeCycleAware;

                if (aware != null)
                {
                    aware.Load(PhoneApplicationService
                        .Current.State);
                }

                return DatabaseCheckResults.Continue;
            }

            page.NavigateTo(AppSettingsService.HasDatabase()
                ? "/Password.xaml" : "/Download.xaml");

            return DatabaseCheckResults.Terminate;
        }

        public static void Closing()
        {
            SaveState();
        }

        public static void Deactivated()
        {
            SaveState();
        }

        public static void Launching()
        {
            LoadState();
        }

        private static void LoadState()
        {
            AppSettingsService.LoadSettings();
        }

        private static void SaveState()
        {
            var aware = ((App)Application.Current)
                .RootFrame.Content as ILifeCycleAware;

            if (aware != null)
            {
                aware.Save(PhoneApplicationService
                    .Current.State);
            }
        }
    }
}