using System;
using System.Net.NetworkInformation;
using System.Windows;
using KeePass.Properties;

namespace KeePass.Utils
{
    internal static class Network
    {
        public static bool CheckNetwork()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
                return true;

            MessageBox.Show(Resources.NoNetwork);
            return false;
        }
    }
}