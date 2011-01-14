using System;
using KeePass.Storage;

namespace KeePass
{
    internal class DatabaseListItemInfo : ListItemInfo
    {
        private readonly DatabaseInfo _database;

        public DatabaseInfo Database
        {
            get { return _database; }
        }

        public DatabaseListItemInfo(DatabaseInfo database)
        {
            if (database == null)
                throw new ArgumentNullException("database");

            _database = database;
            Title = "Test";
        }
    }
}