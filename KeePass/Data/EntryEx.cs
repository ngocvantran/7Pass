using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KeePass.IO.Data;

namespace KeePass.Data
{
    public class EntryEx : INotifyPropertyChanged
    {
        private readonly Entry _entry;
        private readonly IList<FieldValue> _fields;

        private string _notes;
        private string _password;
        private string _title;
        private string _url;
        private string _userName;

        /// <summary>
        /// Occurs when value of and field of <see cref="Fields"/> has changed.
        /// </summary>
        public event FieldChangedEventHandler FieldChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public IList<FieldValue> Fields
        {
            get { return _fields; }
        }

        public string Notes
        {
            get { return _notes; }
            set
            {
                if (value == _notes)
                    return;

                _notes = value;
                OnPropertyChanged("Notes");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value == _password)
                    return;

                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                    return;

                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (value == _url)
                    return;

                _url = value;
                OnPropertyChanged("Url");
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName)
                    return;

                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        public EntryEx(Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            _entry = entry;

            var fields = entry.CustomFields;
            _fields = fields
                .Keys.Select(x => new FieldValue(x, fields))
                .ToList();

            Reset();

            foreach (var field in _fields)
            {
                var key = field.Key;
                field.ValueChanged += (s, e) => OnFieldChanged(
                    new FieldChangedEventArgs(key));
            }
        }

        public bool DetectChanges()
        {
            if (_password != _entry.Password || _userName != _entry.UserName ||
                _title != _entry.Title || _notes != _entry.Notes || _url != _entry.Url)
            {
                return true;
            }

            return Fields.Any(x => x.DetectChanges());
        }

        public void Reset()
        {
            _url = _entry.Url ?? string.Empty;
            _title = _entry.Title ?? string.Empty;
            _notes = _entry.Notes ?? string.Empty;
            _userName = _entry.UserName ?? string.Empty;
            _password = _entry.Password ?? string.Empty;

            foreach (var field in Fields)
                field.Reset();

            OnPropertyChanged("Title");
            OnPropertyChanged("Password");
            OnPropertyChanged("UserName");
            OnPropertyChanged("Url");
            OnPropertyChanged("Notes");
            OnPropertyChanged("CustomFields");
        }

        public void Save()
        {
            _entry.Url = _url;
            _entry.Title = _title;
            _entry.Notes = _notes;
            _entry.UserName = _userName;
            _entry.Password = _password;

            foreach (var field in Fields)
                field.Save();
        }

        /// <summary>
        /// Raises the <see cref="FieldChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnFieldChanged(FieldChangedEventArgs e)
        {
            if (FieldChanged != null)
                FieldChanged(this, e);
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
        }

        public class FieldValue
        {
            private readonly IDictionary<string, string> _fields;
            private readonly string _key;
            private string _value;

            /// <summary>
            /// Occurs when value of <see cref="Value"/> has changed.
            /// </summary>
            public event EventHandler ValueChanged;

            public string Key
            {
                get { return _key; }
            }

            public string Value
            {
                get { return _value; }
                set
                {
                    if (value == _value)
                        return;

                    _value = value;
                    OnValueChanged(EventArgs.Empty);
                }
            }

            public FieldValue(string key,
                IDictionary<string, string> fields)
            {
                if (key == null) throw new ArgumentNullException("key");
                if (fields == null) throw new ArgumentNullException("fields");

                _key = key;
                _fields = fields;

                Reset();
            }

            public bool DetectChanges()
            {
                return _value != _fields[_key];
            }

            public void Reset()
            {
                Value = _fields[_key];
            }

            public void Save()
            {
                _fields[_key] = _value;
            }

            /// <summary>
            /// Raises the <see cref="ValueChanged"/> event.
            /// </summary>
            /// <param name="e">The <see cref="System.EventArgs"/>
            /// instance containing the event data.</param>
            protected virtual void OnValueChanged(EventArgs e)
            {
                if (ValueChanged != null)
                    ValueChanged(this, e);
            }
        }
    }
}