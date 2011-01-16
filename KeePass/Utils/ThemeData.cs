using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KeePass.Utils
{
    internal static class ThemeData
    {
        private static bool _isDarkTheme;
        private static string _suffix;

        public static string GetImage(string name)
        {
            return "/Images/" + name + _suffix;
        }

        public static ImageSource GetImageSource(string name)
        {
            return new BitmapImage(new Uri(
                GetImage(name), UriKind.Relative));
        }

        public static void Initialize()
        {
            var v = (Visibility)Application.Current
                .Resources["PhoneLightThemeVisibility"];
            _isDarkTheme = v != Visibility.Visible;

            _suffix = _isDarkTheme
                ? ".dark.png" : ".light.png";
        }
    }
}