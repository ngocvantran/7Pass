using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Threading;
using KeePass.IO;
using KeePass.IO.Utils;
using KeePass.Utils;
using Newtonsoft.Json;

namespace KeePass.Storage
{
    internal class DatabaseInfo
    {
        /// <summary>
        /// Gets the database's details.
        /// </summary>
        public DatabaseDetails Details { get; private set; }

        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public string Folder { get; set; }

        /// <summary>
        /// Gets a value indicating whether this database has saved password.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this database has saved password; otherwise, <c>false</c>.
        /// </value>
        public bool HasPassword
        {
            get
            {
                using (var store = IsolatedStorageFile
                    .GetUserStoreForApplication())
                {
                    return store.FileExists(ParsedXmlPath);
                }
            }
        }

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
        /// Gets the path to the database information file.
        /// </summary>
        private string InfoPath
        {
            get
            {
                return Path.Combine(
                    Folder, "database.info");
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
        /// Clears the saved password.
        /// </summary>
        public void ClearPassword()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                store.DeleteFile(ProtectionPath);
                store.DeleteFile(ParsedXmlPath);
            }
        }

        /// <summary>
        /// Deletes this database.
        /// </summary>
        public void Delete()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                store.DeleteDirectory(Folder, true);
            }
        }

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
        /// Loads the <see cref="Details"/> data.
        /// </summary>
        public void LoadDetails()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                using (var fs = store.OpenFile(InfoPath, FileMode.Open))
                {
                    var serializer = new JsonSerializer();
                    var reader = new JsonTextReader(
                        new StreamReader(fs));

                    var details = serializer.Deserialize
                        <DatabaseDetails>(reader);

                    Details = details;
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Opens the database using saved password dispatcher.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public void Open(Dispatcher dispatcher)
        {
            var xml = GetSavedPassword();
            Cache.Database = DatabaseReader
                .Load(xml, dispatcher);
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
        /// <param name="details">The details.</param>
        public void SetDatabase(Stream data, DatabaseDetails details)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (details == null) throw new ArgumentNullException("details");

            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (!store.DirectoryExists(Folder))
                    store.CreateDirectory(Folder);

                using (var fs = store.CreateFile(InfoPath))
                {
                    Details = details;

                    var writer = new StreamWriter(fs);
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, details);

                    writer.Flush();
                }

                using (var fs = store.CreateFile(DatabasePath))
                    BufferEx.CopyStream(data, fs);
            }
        }

        /// <summary>
        /// Gets the saved password.
        /// </summary>
        /// <returns></returns>
        private DbPersistentData GetSavedPassword()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                var result = new DbPersistentData();

                using (var fs = store.OpenFile(ProtectionPath, FileMode.Open))
                using (var buffer = new MemoryStream((int)fs.Length))
                {
                    BufferEx.CopyStream(fs, buffer);
                    result.Protection = buffer.ToArray();
                }

                using (var fs = store.OpenFile(ParsedXmlPath, FileMode.Open))
                using (var buffer = new MemoryStream((int)fs.Length))
                {
                    BufferEx.CopyStream(fs, buffer);
                    result.Xml = buffer.ToArray();
                }

                return result;
            }
        }

        /// <summary>
        /// Saves user password.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="xml">The XML.</param>
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