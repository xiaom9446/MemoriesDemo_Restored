using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Foundation.Collections;

namespace MemoriesDemo.Helpers
{
    public partial class CustomVectorView : IReadOnlyList<object>, IEnumerable<object>, IEnumerable
    {
        private readonly List<object> _internalCollection;

        public CustomVectorView(List<object> list)
        {
            _internalCollection = list;
        }

        public object GetAt(uint index)
        {
            return _internalCollection[(int)index];
        }

        public uint GetMany(uint startIndex, object[] items)
        {
            int start = (int)startIndex;
            int copyCount = Math.Min(items.Length, _internalCollection.Count - start);
            if (copyCount <= 0) return 0;
            _internalCollection.CopyTo(start, items, 0, copyCount);
            return (uint)copyCount;
        }

        public bool IndexOf(object value, out uint index)
        {
            int i = _internalCollection.IndexOf(value);
            if (i < 0) { index = 0; return false; }
            index = (uint)i;
            return true;
        }

        public uint Size
        {
            get { return (uint)_internalCollection.Count; }
        }

        public int Count
        {
            get { return _internalCollection.Count; }
        }

        public object this[int index]
        {
            get { return _internalCollection[index]; }
        }

        public IIterator<object> First()
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            return _internalCollection.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return _internalCollection.GetEnumerator();
        }
    }
}