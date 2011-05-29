using System;
using System.IO;
using KeePass.IO.Data;
using KeePass.IO.Utils;

namespace KeePass.IO.Write
{
    public static class DatabaseWriter
    {
        public static void Save(DbPersistentData data, Entry entry)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (entry == null) throw new ArgumentNullException("entry");

            using (var buffer = new MemoryStream(data.Xml))
            {
                var crypto = CryptoSerializer
                    .Deserialize(data.Protection);

                var writer = new XmlWriter();
                writer.Load(buffer);
                writer.Decrypt(crypto.Create());
                writer.Details(entry);
                writer.Encrypt(crypto.Create());
            }
        }
    }
}