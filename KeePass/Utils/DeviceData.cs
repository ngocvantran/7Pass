using System;
using Microsoft.Phone.Info;

namespace KeePass.Utils
{
    internal static class DeviceData
    {
        public static string GetDeviceId()
        {
            return Convert.ToBase64String(
                (byte[])DeviceExtendedProperties
                    .GetValue("DeviceUniqueId"));
        }
    }
}