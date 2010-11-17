using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace KeePass.Data
{
    public class DatabaseItems : INotifyPropertyChanged
    {
        private List<DatabaseItem> _items;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public List<DatabaseItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Items"));
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}