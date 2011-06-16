using System;
using System.Collections;
using System.Collections.Generic;
using NavigationListControl;

namespace KeePass.Controls
{
    public partial class KeePassList
    {
        /// <summary>
        /// Occurs when user tap on an item.
        /// </summary>
        public event EventHandler<NavigationEventArgs> SelectionChanged;

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// <value>
        /// The items source.
        /// </value>
        public IEnumerable ItemsSource
        {
            get { return lstMain.ItemsSource; }
            set { lstMain.ItemsSource = value; }
        }

        public KeePassList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public void SetItems<T>(IEnumerable<T> items)
        {
            lstMain.SetItems(items);
        }

        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="NavigationEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnSelectionChanged(NavigationEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        private void lstMain_Navigation(object sender,
            NavigationEventArgs e)
        {
            OnSelectionChanged(e);
        }
    }
}