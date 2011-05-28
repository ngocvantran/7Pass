using System;
using System.Linq;
using KeePass.Storage;
using Microsoft.Phone.Shell;

namespace KeePass.Utils
{
    internal static class TilesManager
    {
        /// <summary>
        /// Notifies that the database has been deleted.
        /// </summary>
        /// <param name="database">The database.</param>
        public static void Deleted(DatabaseInfo database)
        {
            var uri = GetUri(database);
            var tile = GetTile(uri);

            if (tile != null)
                tile.Delete();
        }

        /// <summary>
        /// Pins the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns><c>true</c> if the database is pinned;
        /// otherwise, <c>false</c>.</returns>
        public static bool Pin(DatabaseInfo database)
        {
            var uri = GetUri(database);

            var tile = GetTile(uri);
            if (tile != null)
                return false;

            var data = GetData(database);
            ShellTile.Create(uri, data);

            return true;
        }

        /// <summary>
        /// Notifies that the database has been renamed.
        /// </summary>
        /// <param name="database">The database.</param>
        public static void Renamed(DatabaseInfo database)
        {
            var uri = GetUri(database);

            var tile = GetTile(uri);
            if (tile == null)
                return;

            var data = GetData(database);
            tile.Update(data);
        }

        private static ShellTileData
            GetData(DatabaseInfo database)
        {
            var background = new Uri(
                "Background.png",
                UriKind.Relative);

            return new StandardTileData
            {
                BackgroundImage = background,
                Title = database.Details.Name,
            };
        }

        private static ShellTile GetTile(Uri uri)
        {
            return ShellTile.ActiveTiles
                .FirstOrDefault(x =>
                    x.NavigationUri == uri);
        }

        private static Uri GetUri(DatabaseInfo db)
        {
            return Navigation.GetPathTo<MainPage>(
                "tile={0}", db.Folder);
        }
    }
}