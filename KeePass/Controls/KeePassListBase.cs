using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using NavigationListControl;

namespace KeePass.Controls
{
    public class KeePassListBase : NavigationList
    {
        /// <summary>
        /// Sets the items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public void SetItems<T>(IEnumerable<T> items)
        {
            ThreadPool.QueueUserWorkItem(
                _ => SetItemsPrivate(items));
        }

        private void SetItemsPrivate<T>(IEnumerable<T> items)
        {
            items = items.ToList();
            var dispatcher = Dispatcher;
            var wait = new ManualResetEvent(false);
            var list = new ObservableCollection<T>();

            dispatcher.BeginInvoke(() =>
            {
                ItemsSource = list;
                wait.Set();
            });

            wait.WaitOne();

            var waits = items.Take(10);
            foreach (var item in waits)
            {
                var local = item;
                wait.Reset();

                dispatcher.BeginInvoke(() =>
                {
                    list.Add(local);
                    wait.Set();
                });
                wait.WaitOne();
            }

            var remaining = items
                .Skip(10)
                .ToList();

            if (remaining.Count > 0)
            {
                dispatcher.BeginInvoke(() =>
                {
                    foreach (var item in remaining)
                        list.Add(item);
                });
            }
        }
    }
}