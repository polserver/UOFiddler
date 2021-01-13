using System;
using System.Collections.Generic;

namespace UoFiddler.Controls.UserControls.TileView
{
    public class IndicesCollection : IList<int>
    {
        private readonly List<int> _internal = new List<int>();

        public int IndexOf(int item)
        {
            return _internal.IndexOf(item);
        }

        public void Add(int item)
        {
            _internal.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int>() { item }));
        }

        public void Insert(int index, int item)
        {
            _internal.Insert(index, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int>() { item }));
        }

        public int this[int index]
        {
            get => _internal[index];
            set
            {
                _internal[index] = value;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int>() { value }));
            }
        }

        public void Clear()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _internal));

            _internal.Clear();
        }

        public bool Contains(int item)
        {
            return _internal.Contains(item);
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            _internal.CopyTo(array, arrayIndex);
        }

        public int Count => _internal.Count;

        public bool IsReadOnly => false;

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _internal.Count)
                throw new IndexOutOfRangeException("Index out of range");

            int item = this[index];

            _internal.RemoveAt(index);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<int>() { item }));
        }

        public bool Remove(int item)
        {
            bool removed = _internal.Remove(item);

            if (removed)
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<int>() { item }));

            return removed;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

        public class NotifyCollectionChangedEventArgs : EventArgs
        {
            public NotifyCollectionChangedAction Action;
            public List<int> ItemsChanged;

            /// <summary>
            /// Initializes NotifyCollectionChangedEventArgs on Add or Remove action with list of changed items
            /// </summary>
            /// <param name="action"></param>
            /// <param name="itemsChanged"></param>
            public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, List<int> itemsChanged)
            {
                Action = action;
                ItemsChanged = itemsChanged;
            }
        }

        public enum NotifyCollectionChangedAction
        {
            Add,
            Remove
        }
    }
}
