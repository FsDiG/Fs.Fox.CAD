using System;
using System.Linq;
using System.Collections.Generic;

namespace ThCADExtension
{
    public interface IDisposableCollection<T> : ICollection<T>, IDisposable
       where T : IDisposable
    {
        void AddRange(IEnumerable<T> items);
        IEnumerable<T> RemoveRange(IEnumerable<T> items);
    }

    public class DisposableSet<T> : HashSet<T>, IDisposableCollection<T>
       where T : IDisposable
    {
        public DisposableSet()
        {
        }

        public DisposableSet(IEnumerable<T> items)
        {
            AddRange(items);
        }

        public void Dispose()
        {
            if (base.Count > 0)
            {
                System.Exception last = null;
                var list = this.ToList();
                this.Clear();
                foreach (T item in list)
                {
                    if (item != null)
                    {
                        try
                        {
                            item.Dispose();
                        }
                        catch (System.Exception ex)
                        {
                            last = last ?? ex;
                        }
                    }
                }
                if (last != null)
                    throw last;
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            base.UnionWith(items);
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            base.ExceptWith(items);
            return items;
        }
    }
}
