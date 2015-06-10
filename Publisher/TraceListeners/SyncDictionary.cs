using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Parcsis.PSD.Publisher.TraceListeners
{
    public class SyncDictionary<K,T>: SynchronizedKeyedCollection<K,T>
        where K:class
    {
        
        public SyncDictionary():base()
        {
            if (Dictionary == null)
            {
                MethodInfo mi = this.GetType().BaseType.GetMethod("CreateDictionary", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                mi.Invoke(this, null);
            }
        }
        
        public T Add(K key, T item)
        {
            lock (SyncRoot)
            {
                this.Dictionary.Add(key, item);
            }
            return item;
        }
        
        protected override K GetKeyForItem(T item)
        {
            K key = null;
            lock (SyncRoot)
            {
                foreach (KeyValuePair<K, T> pair in this.Dictionary)
                {
                    if (pair.Value.Equals(item))
                    {
                        key = pair.Key;
                        break;
                    }
                }
            }
            return key;
        }

        public bool ContainsKey(K key)
        {
            lock (SyncRoot)
            {
                return this.Dictionary.ContainsKey(key);
            }
        }
    }
}
