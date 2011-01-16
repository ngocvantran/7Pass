using System;
using System.ComponentModel;
using System.Windows.Media;
using KeePass.Storage;

namespace KeePass.Data
{
    public class DatabaseItem : INotifyPropertyChanged
    {
        private readonly DatabaseInfo _info;
        private bool _hasPassword;
        private bool _isUpdating;
        private ImageSource _passwordIcon;
        private ImageSource _updatedIcon;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanUpdate
        {
            get { return !string.IsNullOrEmpty(_info.Details.Url); }
        }

        public bool HasPassword
        {
            get { return _hasPassword; }
            set
            {
                if (value == _hasPassword)
                    return;

                _hasPassword = value;
                OnPropertyChanged("HasPassword");
            }
        }

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
                OnPropertyChanged("IsUpdating");
            }
        }

        public string Name
        {
            get { return _info.Details.Name; }
        }

        public ImageSource PasswordIcon
        {
            get { return _passwordIcon; }
            set
            {
                _passwordIcon = value;
                OnPropertyChanged("PasswordIcon");
            }
        }

        public ImageSource UpdatedIcon
        {
            get { return _updatedIcon; }
            set
            {
                _updatedIcon = value;
                OnPropertyChanged("UpdatedIcon");
            }
        }

        internal DatabaseItem(DatabaseInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            _info = info;
            _hasPassword = info.HasPassword;

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

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}