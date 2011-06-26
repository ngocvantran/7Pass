using System;
using System.Windows.Controls;
using Coding4Fun.Phone.Controls;

namespace KeePass.Storage
{
    internal static class SaveUtils
    {
        /// <summary>
        /// Notifies the user if the database cannot be synchronized.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns><c>true</c> if the database cannot
        /// be synchronized; otherwise, <c>false</c>.</returns>
        public static bool NotifyIfNotSyncable(
            this DatabaseInfo db)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            var details = db.Details;
            if (details == null || details.Type == SourceTypes.Synchronizable)
                return false;

            new ToastPrompt
            {
                Title = Resources.NotSyncableTitle,
                Message = Resources.NotSyncableCaption,
                TextOrientation = Orientation.Vertical,
            }.Show();

            return true;
        }
    }
}