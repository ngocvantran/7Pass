using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using KeePass.IO;

namespace KeePass.Storage
{
    internal static class DatabaseVerifier
    {
        public static bool Verify(Dispatcher dispatcher, Stream file)
        {
            if (file.Length == 0)
            {
                dispatcher.BeginInvoke(() =>
                    MessageBox.Show(Resources.EmptyFile,
                        Properties.Resources.DownloadTitle,
                        MessageBoxButton.OK));

                return false;
            }

            if (!DatabaseReader.CheckSignature(file))
            {
                dispatcher.BeginInvoke(() =>
                    MessageBox.Show(Resources.NotKdbx,
                        Properties.Resources.DownloadTitle,
                        MessageBoxButton.OK));

                return false;
            }

            file.Position = 0;
            return true;
        }

        public static string VerifyUnattened(Stream file)
        {
            if (file.Length == 0)
                return Resources.EmptyFile;

            if (!DatabaseReader.CheckSignature(file))
                return Resources.NotKdbx;

            file.Position = 0;
            return null;
        }
    }
}