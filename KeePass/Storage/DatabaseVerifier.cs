using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using KeePass.IO.Read;

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

            var oddTransforms = DatabaseReader
                .LargeTransformRounds(file);

            if (oddTransforms)
            {
                dispatcher.BeginInvoke(() =>
                    MessageBox.Show(
                        Properties.Resources.LargeTransforms,
                        Properties.Resources.LargeTransformsTitle,
                        MessageBoxButton.OK));
            }

            file.Position = 0;
            return true;
        }

        public static VerifyResults VerifyUnattened(Stream file)
        {
            if (file.Length == 0)
            {
                return new VerifyResults
                {
                    Message = Resources.EmptyFile,
                    Result = VerifyResultTypes.Error,
                };
            }

            if (!DatabaseReader.CheckSignature(file))
            {
                return new VerifyResults
                {
                    Message = Resources.NotKdbx,
                    Result = VerifyResultTypes.Error,
                };
            }

            var oddTransforms = DatabaseReader
                .LargeTransformRounds(file);

            if (oddTransforms)
            {
                return new VerifyResults
                {
                    Message = Properties.Resources
                        .LargeTransforms,
                    Result = VerifyResultTypes.Warning,
                };
            }

            file.Position = 0;

            return new VerifyResults
            {
                Result = VerifyResultTypes.Pass,
            };
        }
    }
}