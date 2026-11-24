using System;
using System.Collections.Generic;

namespace MemoriesDemo.Helpers
{
    public partial class WeakReferenceEvent<T1, T2>
    {
        private readonly List<WeakReference> _delegates = new List<WeakReference>();

        public static WeakReferenceEvent<T1, T2> operator +(WeakReferenceEvent<T1, T2> wre, Action<T1, T2> handler)
        {
            wre.Add(handler);
            return wre;
        }

        public static WeakReferenceEvent<T1, T2> operator -(WeakReferenceEvent<T1, T2> wre, Action<T1, T2> handler)
        {
            wre.Remove(handler);
            return wre;
        }

        public void Add(Action<T1, T2> handler)
        {
            if (handler == null) return;

            foreach (var wr in _delegates)
            {
                if (wr.Target == handler)
                    return;
            }
            _delegates.Add(new WeakReference(handler));
        }

        public void Remove(Action<T1, T2> handler)
        {
            if (handler == null) return;

            WeakReference found = null;
            foreach (var wr in _delegates)
            {
                if (wr.Target == handler)
                {
                    found = wr;
                    break;
                }
            }
            if (found != null)
            {
                _delegates.Remove(found);
            }
        }

        public void Invoke(T1 sender, T2 arg)
        {
            List<WeakReference> dead = null;

            foreach (var wr in _delegates)
            {
                if (wr.IsAlive)
                {
                    var action = wr.Target as Action<T1, T2>;
                    if (action != null)
                    {
                        action.Invoke(sender, arg);
                    }
                }
                else
                {
                    if (dead == null)
                    {
                        dead = new List<WeakReference>();
                    }
                    dead.Add(wr);
                }
            }

            if (dead != null)
            {
                foreach (var wr in dead)
                {
                    _delegates.Remove(wr);
                }
            }
        }
    }
}