using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace MemoriesDemo.Helpers
{
    public partial class ObservableList : IObservableVector<object>,
        IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable
    {
        private readonly List<object> _internalCollection;
        private readonly CustomVectorView _customVectorView;

        public ObservableList(List<object> list)
        {
            _internalCollection = list;
            _customVectorView = new CustomVectorView(list);
        }

        // WinRT 接口事件：用 field-like event 隐式实现，
        // 编译器会自动生成返回 EventRegistrationToken 的 add/remove 桥接。
        public event VectorChangedEventHandler<object> VectorChanged;

        private void RaiseChanged(CollectionChange change, uint index)
        {
            var handler = this.VectorChanged;
            if (handler != null)
            {
                handler(this, new CustomVectorChangedEventArgs
                {
                    CollectionChange = change,
                    Index = index
                });
            }
        }

        // ---- IList<object> ----
        public int IndexOf(object item) { return _internalCollection.IndexOf(item); }

        public void Insert(int index, object item)
        {
            _internalCollection.Insert(index, item);
            RaiseChanged(CollectionChange.ItemInserted, (uint)index);
        }

        public void RemoveAt(int index)
        {
            _internalCollection.RemoveAt(index);
            RaiseChanged(CollectionChange.ItemRemoved, (uint)index);
        }

        public object this[int index]
        {
            get { return _internalCollection[index]; }
            set
            {
                _internalCollection[index] = value;
                RaiseChanged(CollectionChange.ItemChanged, (uint)index);
            }
        }

        public void Add(object item)
        {
            _internalCollection.Add(item);
            RaiseChanged(CollectionChange.ItemInserted, (uint)(_internalCollection.Count - 1));
        }

        public void Clear()
        {
            _internalCollection.Clear();
            RaiseChanged(CollectionChange.Reset, 0);
        }

        public bool Contains(object item) { return _internalCollection.Contains(item); }
        public void CopyTo(object[] array, int arrayIndex) { _internalCollection.CopyTo(array, arrayIndex); }
        public int Count { get { return _internalCollection.Count; } }
        public bool IsReadOnly { get { return false; } }

        public bool Remove(object item)
        {
            int idx = _internalCollection.IndexOf(item);
            if (idx < 0) return false;
            _internalCollection.RemoveAt(idx);
            RaiseChanged(CollectionChange.ItemRemoved, (uint)idx);
            return true;
        }

        // ---- 枚举 ----
        public IEnumerator GetEnumerator() { return _internalCollection.GetEnumerator(); }
        IEnumerator<object> IEnumerable<object>.GetEnumerator() { return _internalCollection.GetEnumerator(); }

        // ---- IVector 显式接口 ----
        public void Append(object value) { Add(value); }
        public object GetAt(uint index) { return _internalCollection[(int)index]; }
        public uint GetMany(uint startIndex, object[] items) { return _customVectorView.GetMany(startIndex, items); }
        public IReadOnlyList<object> GetView() { return _customVectorView; }

        public bool IndexOf(object value, out uint index)
        {
            int i = _internalCollection.IndexOf(value);
            if (i < 0) { index = 0; return false; }
            index = (uint)i;
            return true;
        }

        public void InsertAt(uint index, object value) { Insert((int)index, value); }

        public void RemoveAt(uint index)
        {
            _internalCollection.RemoveAt((int)index);
            RaiseChanged(CollectionChange.ItemRemoved, index);
        }

        public void RemoveAtEnd()
        {
            int lastIndex = _internalCollection.Count - 1;
            _internalCollection.RemoveAt(lastIndex);
            RaiseChanged(CollectionChange.ItemRemoved, (uint)lastIndex);
        }

        public void ReplaceAll(object[] items)
        {
            _internalCollection.Clear();
            _internalCollection.AddRange(items);
            RaiseChanged(CollectionChange.Reset, 0);
        }

        public void SetAt(uint index, object value) { this[(int)index] = value; }
        public uint Size { get { return (uint)_internalCollection.Count; } }
    }
}