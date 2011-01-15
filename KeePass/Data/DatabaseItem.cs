using System;
using System.ComponentModel;
using KeePass.Storage;

namespace KeePass.Data
{
    public class DatabaseItem : INotifyPropertyChanged
    {
        private readonly DatabaseInfo _info;

        private bool _isUpdating;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public object Info
        {
            get { return _info; }
        }

        public bool IsUpdating
        {
            get { return _isUpdating; }
            set
            {
                if (value == _isUpdating)
                    return;

                _isUpdating = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsUpdating"));
            }
        }

        public string Name
        {
            get { return _info.Details.Name; }
        }

        internal DatabaseItem(DatabaseInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            _info = info;

            if (info.Details == null)
                info.LoadDetails();
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}