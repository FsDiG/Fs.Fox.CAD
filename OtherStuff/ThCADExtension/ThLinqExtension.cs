using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThCADExtension
{
    public static class DictionaryExtensions
    {
        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic, 
            Func<TKey, TValue, bool> predicate)
        {
            var keys = dic.Keys.Where(k => predicate(k, dic[k])).ToList();
            foreach (var key in keys)
            {
                dic.Remove(key);
            }
        }
    }
}
