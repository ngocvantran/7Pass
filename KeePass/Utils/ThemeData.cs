using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KeePass.Utils
{
    internal static class ThemeData
    {
        private static bool? _isDarkTheme;
        private static string _suffix;

        public static string Suffix
        {
            get
            {
                if (_suffix == null)
                {
                    _suffix = IsDarkTheme()
                        ? ".dark.png"
                        : ".light.png";
                }

                return _suffix;
            }
        }

        public static string GetImage(string name)
        {
            return "/Images/" + name + Suffix;
        }

        public static ImageSource GetImageSource(string name)
        {
            return new BitmapImage(new Uri(
                GetImage(name), UriKind.Relative));
        }

        public static bool IsDarkTheme()
        {
            if (_isDarkTheme == null)
            {
                var v = (Visibility)Application.Current
                    .Resources["PhoneLightThemeVisibility"];
                _isDarkTheme = v != Visibility.Visible;
            }

            return _isDarkTheme.Value;
        }
    }
}