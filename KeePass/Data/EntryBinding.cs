using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KeePass.IO.Data;

namespace KeePass.Data
{
    public class EntryBinding : INotifyPropertyChanged
    {
        private readonly List<Field> _addedFields;
        private readonly Entry _entry;

        private bool _hasChanges;

        /// <summary>
        /// Occurs when value of <see cref="HasChanges"/> has changed.
        /// </summary>
        public event EventHandler HasChangesChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                if (value == _hasChanges)
                    return;

                _hasChanges = value;
                OnHasChangesChanged(EventArgs.Empty);
            }
        }

        public string Notes
        {
            get { return _entry.Notes ?? string.Empty; }
            set
            {
                if (value == _entry.Notes)
                    return;

                _entry.Notes = value;
                OnPropertyChanged("Notes");
            }
        }

        public string Password
        {
            get { return _entry.Password ?? string.Empty; }
            set
            {
                if (value == _entry.Password)
                    return;

                _entry.Password = value;
                OnPropertyChanged("Password");
            }
        }

        public string Title
        {
            get { return _entry.Title ?? string.Empty; }
            set
            {
                if (value == _entry.Title)
                    return;

                _entry.Title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Url
        {
            get { return _entry.Url ?? string.Empty; }
            set
            {
                if (value == _entry.Url)
                    return;

                _entry.Url = value;
                OnPropertyChanged("Url");
            }
        }

        public string UserName
        {
            get { return _entry.UserName ?? string.Empty; }
            set
            {
                if (value == _entry.UserName)
                    return;

                _entry.UserName = value;
                OnPropertyChanged("UserName");
            }
        }

        public EntryBinding(Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            _entry = entry;
            _addedFields = new List<Field>();
        }

        public Field AddField()
        {
            var field = new Field();
            _addedFields.Add(field);

            return field;
        }

        public IList<Field> GetFields()
        {
            return _entry.CustomFields;
        }

        public void Reset()
        {
            _entry.Reset();
            _addedFields.Clear();

            HasChanges = false;
        }

        public void Save()
        {
            foreach (var field in _addedFields
                .Where(x => !string.IsNullOrEmpty(x.Name)))
            {
                _entry.Add(field);
            }
        }

        /// <summary>
        /// Raises the <see cref="HasChangesChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnHasChangesChanged(EventArgs e)
        {
            if (HasChangesChanged != null)
                HasChangesChanged(this, e);
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

        private void OnPropertyChanged(string property)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(property));
            HasChanges = true;
        }
    }
}