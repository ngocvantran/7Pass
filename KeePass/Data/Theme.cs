using System;
using System.Windows;

namespace KeePass.Data
{
    internal static class Theme
    {
        public static bool IsDarkTheme()
        {
            var v = (Visibility)Application.Current
                .Resources["PhoneLightThemeVisibility"];
            return v != Visibility.Visible;
        }
    }
}