using System;
using KeePass.Storage;

namespace KeePass.Sources
{
    internal abstract class ServiceAdapterBase : IServiceAdapter
    {
        public event SynchronizeErrorEventHandler Error;

        public abstract void Conflict(ListItem item,
            Action<ListItem, string, string> uploaded);

        public abstract void Download(ListItem item,
            Action<ListItem, byte[]> downloaded);

        public abstract SyncInfo Initialize(DatabaseInfo info);

        public abstract void List(Action<ListItem> ready);

        public abstract void Upload(ListItem item,
            Action<ListItem> uploaded);

        /// <summary>
        /// Raises the <see cref="Error"/> event.
        /// </summary>
        /// <param name="e">The <see cref="SynchronizeErrorEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnError(SynchronizeErrorEventArgs e)
        {
            if (Error != null)
                Error(this, e);
        }

        protected void OnError()
        {
            OnError(new SynchronizeErrorEventArgs(
                new SynchronizeException()));
        }

        protected void OnError(Exception ex)
        {
            OnError(new SynchronizeErrorEventArgs(
                new SynchronizeException("Sync Error", ex)));
        }
    }
}