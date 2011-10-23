using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using KeePass.IO.Data;
using KeePass.Properties;
using KeePass.Utils;

namespace KeePass.Data
{
    public class FieldBinding : INotifyPropertyChanged
    {
        private readonly Field _field;
        private readonly IList<FieldBinding> _items;

        private bool _editing;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource EditImage
        {
            get { return ThemeData.GetImageSource("edit"); }
        }

        public Visibility EditVisibility
        {
            get
            {
                return _editing
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public bool IsEditing
        {
            get { return _editing; }
            set
            {
                if (value == _editing)
                    return;

                _editing = value;
                OnPropertyChanged("IsEditing");
                OnPropertyChanged("EditVisibility");
                OnPropertyChanged("NameLabelVisibility");
            }
        }

        public IList<FieldBinding> Items
        {
            get { return _items; }
        }

        public string Name
        {
            get { return NotEmpty(_field.Name); }
            set
            {
                var name = NotEmpty(value);
                if (name == _field.Name)
                    return;

                _field.Name = name;
                OnPropertyChanged("Name");
            }
        }

        public Visibility NameLabelVisibility
        {
            get
            {
                return _editing
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public bool Protected
        {
            get { return _field.Protected; }
            set
            {
                if (value == _field.Protected)
                    return;

                _field.Protected = value;
                OnPropertyChanged("Protected");
            }
        }

        public string Value
        {
            get { return _field.Value; }
            set
            {
                if (value == _field.Value)
                    return;

                _field.Value = value;
                OnPropertyChanged("Value");
            }
        }

        public FieldBinding(Field field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            _field = field;
            _items = new[] {this};
            _field.Name = NotEmpty(_field.Name);
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

        protected void OnPropertyChanged(string property)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(property));
        }

        private static string NotEmpty(string value)
        {
            return !string.IsNullOrEmpty(value)
                ? value : Resources.EmptyField;
        }
    }
}