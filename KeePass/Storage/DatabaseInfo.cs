using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Threading;
using KeePass.IO;
using KeePass.IO.Utils;

namespace KeePass.Storage
{
    internal class DatabaseInfo
    {
        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public string Folder { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the path to database file.
        /// </summary>
        private string DatabasePath
        {
            get
            {
                return Path.Combine(
                    Folder, "database.kdbx");
            }
        }

        /// <summary>
        /// Gets the path to the parsed database file.
        /// </summary>
        private string ParsedXmlPath
        {
            get
            {
                return Path.Combine(
                    Folder, "database.xml");
            }
        }

        /// <summary>
        /// Gets the path to the protection data file.
        /// </summary>
        private string ProtectionPath
        {
            get
            {
                return Path.Combine(
                    Folder, "protect.bin");
            }
        }

        public DatabaseInfo(string folder)
        {
            Folder = folder;
        }

        public DatabaseInfo()
            : this(Guid.NewGuid().ToString("N")) {}

        /// <summary>
        /// Gets all databases.
        /// </summary>
        /// <returns>All databases.</returns>
        public static IList<DatabaseInfo> GetAll()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                return store.GetDirectoryNames()
                    .Select(x => new DatabaseInfo(x))
                    .ToList();
            }
        }

        /// <summary>
        /// Opens the database using the specified password.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="password">The password.</param>
        /// <param name="savePassword">set to <c>true</c> to save user password.</param>
        /// <returns>Open result.</returns>
        public OpenDbResults Open(Dispatcher dispatcher,
            string password, bool savePassword)
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                using (var fs = store.OpenFile(
                    DatabasePath, FileMode.Open))
                {
                    DbPersistentData xml;

                    try
                    {
                        xml = DatabaseReader
                            .GetXml(fs, password);
                    }
                    catch
                    {
                        return OpenDbResults.CorruptedFile;
                    }

                    if (xml == null)
                        return OpenDbResults.IncorrectPassword;

                    if (savePassword)
                        Save(store, xml);

                    Cache.Database = DatabaseReader
                        .Load(xml, dispatcher);

                    return OpenDbResults.Success;
                }
            }
        }

        /// <summary>
        /// Sets the database.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SetDatabase(Stream data)
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (!store.DirectoryExists(Folder))
                    store.CreateDirectory(Folder);

                using (var fs = store.CreateFile(DatabasePath))
                    BufferEx.CopyStream(data, fs);
            }
        }

        private void Save(IsolatedStorageFile store,
            DbPersistentData xml)
        {
            if (!store.DirectoryExists(Folder))
                store.CreateDirectory(Folder);

            using (var fs = store.CreateFile(ProtectionPath))
            {
                var protect = xml.Protection;
                fs.Write(protect, 0, protect.Length);
            }

            using (var fs = store.CreateFile(ParsedXmlPath))
            using (var buffer = new MemoryStream(xml.Xml))
                BufferEx.CopyStream(buffer, fs);
        }
    }
}