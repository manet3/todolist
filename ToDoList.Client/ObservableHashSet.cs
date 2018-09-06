using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ToDoList.Client
{
    public class ObservableHashSet<T> : ObservableCollection<T>
    {
        private HashSet<T> _hashSet;

        public ObservableHashSet() : base()
            => _hashSet = new HashSet<T>();

        public ObservableHashSet(IEnumerable<T> collection)
            : base(collection)
            => _hashSet = new HashSet<T>(collection);

        public new bool Add(T item)
        {
            var isUnique = _hashSet.Add(item);

            if (isUnique)
                base.Add(item);

            return isUnique;
        }

        public new bool Remove(T item)
        {
            var existingItem = _hashSet.FirstOrDefault(x => x.Equals(item));

            if (existingItem == null)
                return false;

            base.Remove(existingItem);
            _hashSet.Remove(existingItem);

            return true;
        }

        public static implicit operator HashSet<T>(ObservableHashSet<T> observableHashSet)
            => new HashSet<T>(observableHashSet);
    }
}
