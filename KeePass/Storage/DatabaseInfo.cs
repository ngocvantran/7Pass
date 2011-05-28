using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Threading;
using KeePass.IO;
using KeePass.IO.Utils;
using KeePass.Sources;
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
        /// Gets a value indicating whether this database has key file.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this database has key file; otherwise, <c>false</c>.
        /// </value>
        public bool HasKeyFile
        {
            get
            {
                using (var store = IsolatedStorageFile
                    .GetUserStoreForApplication())
                {
                    return store.FileExists(KeyFilePath);
                }
            }
        }

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
        /// Gets the key file path.
        /// </summary>
        private string KeyFilePath
        {
            get
            {
                return Path.Combine(
                    Folder, "keyfile.bin");
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
                ClearPassword(store);
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
                Upgrade(store);

                return store.GetDirectoryNames()
                    .Select(x => new DatabaseInfo(x))
                    .Where(x => x.IsValid(store))
                    .ToList();
            }
        }

        /// <summary>
        /// Determines whether the this database has stored data.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <returns><c>true</c> if this database is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid(IsolatedStorageFile store)
        {
            return store.FileExists(InfoPath);
        }

        /// <summary>
        /// Loads the <see cref="Details"/> data.
        /// </summary>
        public void LoadDetails()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                LoadDetails(store);
            }
        }

        /// <summary>
        /// Opens the database using saved password dispatcher.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public void Open(Dispatcher dispatcher)
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                var xml = GetSavedPassword(store);
                Open(store, xml, dispatcher);
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
                        xml = DatabaseReader.GetXml(fs,
                            password, GetKeyFile(store));
                    }
                    catch
                    {
                        return OpenDbResults.CorruptedFile;
                    }

                    if (xml == null)
                        return OpenDbResults.IncorrectPassword;

                    if (savePassword)
                        Save(store, xml);
                    else
                        ClearPassword();

                    Open(store, xml, dispatcher);
                    return OpenDbResults.Success;
                }
            }
        }

        /// <summary>
        /// Opens the key file.
        /// </summary>
        /// <param name="action">The action.</param>
        public void OpenKeyFile(Action<Stream> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                using (var fs = store.OpenFile(KeyFilePath, FileMode.Open))
                    action(fs);
            }
        }

        /// <summary>
        /// Saves the <see cref="Details"/> information.
        /// </summary>
        public void SaveDetails()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                SaveDetails(store);
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

                ClearPassword(store);

                Details = details;
                SaveDetails(store);

                using (var fs = store.CreateFile(DatabasePath))
                    BufferEx.CopyStream(data, fs);
            }
        }

        /// <summary>
        /// Sets the key file.
        /// </summary>
        /// <param name="keyFile">The key file.</param>
        public void SetKeyFile(byte[] keyFile)
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                var path = KeyFilePath;
                if (store.FileExists(path))
                    store.DeleteFile(path);

                if (keyFile == null)
                    return;

                using (var fs = store.CreateFile(path))
                    fs.Write(keyFile, 0, keyFile.Length);
            }
        }

        private void ClearPassword(IsolatedStorageFile store)
        {
            store.DeleteFile(ProtectionPath);
            store.DeleteFile(ParsedXmlPath);
        }

        private byte[] GetKeyFile(IsolatedStorageFile store)
        {
            var path = KeyFilePath;
            if (!store.FileExists(path))
                return null;

            using (var fs = store.OpenFile(path, FileMode.Open))
            {
                var length = (int)fs.Length;
                var keyFile = new byte[length];
                fs.Read(keyFile, 0, length);

                return keyFile;
            }
        }

        /// <summary>
        /// Gets the saved password.
        /// </summary>
        /// <returns></returns>
        private DbPersistentData GetSavedPassword(
            IsolatedStorageFile store)
        {
            return GetSavedPassword(store,
                ProtectionPath, ParsedXmlPath);
        }

        /// <summary>
        /// Gets the saved password.
        /// </summary>
        /// <returns></returns>
        private static DbPersistentData GetSavedPassword(
            IsolatedStorageFile store,
            string protectPath, string parsedXmlPath)
        {
            var result = new DbPersistentData();

            using (var fs = store.OpenFile(protectPath, FileMode.Open))
            using (var buffer = new MemoryStream((int)fs.Length))
            {
                BufferEx.CopyStream(fs, buffer);
                result.Protection = buffer.ToArray();
            }

            using (var fs = store.OpenFile(parsedXmlPath, FileMode.Open))
            using (var buffer = new MemoryStream((int)fs.Length))
            {
                BufferEx.CopyStream(fs, buffer);
                result.Xml = buffer.ToArray();
            }

            return result;
        }

        private void LoadDetails(IsolatedStorageFile store)
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

        private void Open(IsolatedStorageFile store,
            DbPersistentData xml, Dispatcher dispatcher)
        {
            var database = DatabaseReader
                .Load(xml, dispatcher);

            var name = HasPassword
                ? Folder : string.Empty;

            if (Details == null)
                LoadDetails(store);

            Cache.CacheDb(this, name, database);
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

        private void SaveDetails(IsolatedStorageFile store)
        {
            using (var fs = store.CreateFile(InfoPath))
            {
                var writer = new StreamWriter(fs);
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, Details);

                writer.Flush();
            }
        }

        private static void Upgrade(IsolatedStorageFile store)
        {
            var files = new List<string>(
                store.GetFileNames());

            if (!files.Contains("Database.kdbx"))
                return;

            var appSettings = IsolatedStorageSettings
                .ApplicationSettings;

            string url;
            if (!appSettings.TryGetValue("Url", out url))
                url = null;

            var info = new DatabaseInfo();

            using (var fs = store.OpenFile("Database.kdbx", FileMode.Open))
            {
                var source = string.IsNullOrEmpty(url)
                    ? "7Pass" : DatabaseUpdater.WEB_UPDATER;

                var details = new DatabaseDetails
                {
                    Url = url,
                    Source = source,
                    Name = "7Pass 1.x database",
                };

                info.SetDatabase(fs, details);
            }

            store.DeleteFile("Database.kdbx");

            if (!files.Contains("Decrypted.xml"))
                return;

            var password = GetSavedPassword(store,
                "Protection.bin", "Decrypted.xml");

            info.Save(store, password);

            store.DeleteFile("Protection.bin");
            store.DeleteFile("Decrypted.xml");
        }
    }
}