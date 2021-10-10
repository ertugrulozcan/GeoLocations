#nullable disable

using System;
using System.Collections.Generic;

namespace GeoLocations.MMDB
{
	#region Delegations

	/// <summary>
	/// Delegate that can be used to be notified when an item is removed from a CachedDictionary because the size was too big
	/// </summary>
	internal delegate void CachedItemRemovedDelegate<TKey, TValue>(CachedDictionary<TKey, TValue> dictionary, TKey key, TValue value);

	#endregion
	
    /// <summary>
    /// A dictionary that caches up to N values in memory. Once the dictionary reaches N count, the last item in the internal list is removed.
    /// New items are always added to the start of the internal list.
    /// </summary>
    internal class CachedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
		#region Fields

		private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dictionary;
		private LinkedList<KeyValuePair<TKey, TValue>> priorityList;
		private int maxCount;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="maxCount_">Maximum count the in memory dictionary will be allowed to grow to</param>
		/// <param name="comparer">Comparer for TKey (can be null for default)</param>
		public CachedDictionary(int maxCount_, IEqualityComparer<TKey> comparer)
		{
			if (maxCount_ < 1)
			{
				throw new ArgumentOutOfRangeException("Maxcount is " + maxCount_ + ", it must be greater than 0");
			}
			
			if (comparer == null)
			{
				comparer = EqualityComparer<TKey>.Default;
			}
			
			this.maxCount = maxCount_;
			this.SetComparer(comparer);
		}

		#endregion

		#region Methods

		private void MoveToFront(LinkedListNode<KeyValuePair<TKey, TValue>> node)
		{
			this.priorityList.Remove(node);
			this.priorityList.AddFirst(node);
		}

		private void InternalAdd(TKey key, TValue value)
		{
			if (this.dictionary.Count == maxCount && this.priorityList.Last != null)
			{
				this.dictionary.Remove(this.priorityList.Last.Value.Key);
				this.priorityList.RemoveLast();
			}
			
			this.priorityList.AddFirst(new KeyValuePair<TKey, TValue>(key, value));
			this.dictionary.Add(key, priorityList.First);
		}

		private bool InternalRemove(TKey key)
		{
			if (!this.dictionary.TryGetValue(key, out var node))
				return this.OnRemoveExternalKey(key);
			
			this.priorityList.Remove(node);
			this.dictionary.Remove(key);
			
			return true;

		}
		
		/// <summary>
		/// Fires when a key is not found in the in memory dictionary. This gives derived classes an opportunity to look in external sources like
		/// files or databases for the value that key represents. If the derived class finds a value matching the key in the external source,
		/// then the derived class can set value and return true; when this happens the newly added value is added to the priority list.
		/// </summary>
		/// <param name="key">Key (can be replaced by the found key if desired)</param>
		/// <param name="value">Value that was found</param>
		/// <returns>True if found from external source, false if not</returns>
		protected virtual bool OnGetExternalKeyValue(ref TKey key, out TValue value)
		{
			value = default;
			return false;
		}

		/// <summary>
		/// Sets a new comparer. Clears the cache.
		/// </summary>
		/// <param name="comparer">New comparer</param>
		protected void SetComparer(IEqualityComparer<TKey> comparer)
		{
			this.dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
			this.priorityList = new LinkedList<KeyValuePair<TKey, TValue>>();
		}

		/// <summary>
		/// Removes an external key. The key will have already been normalized. This implementation does nothing.
		/// </summary>
		/// <param name="key">Key to remove</param>
		/// <returns>True if the key was removed, false if not</returns>
		protected virtual bool OnRemoveExternalKey(TKey key)
		{
			return false;
		}

		#endregion

        #region IDictionary<TKey,TValue> Members

		/// <summary>
        /// Adds a key / value pair to the dictionary. If the key already exists, it's value is replaced and moved to the front.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        public void Add(TKey key, TValue value)
        {
			this.InternalRemove(key);
			this.InternalAdd(key, value);
        }

        /// <summary>
        /// Checks to see if the given key is in the dictionary by calling TryGetValue.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if in dictionary, false if not</returns>
        public bool ContainsKey(TKey key)
        {
            return this.TryGetValue(key, out _);
        }

        /// <summary>
        /// Removes a key from memory. If there is an external source, the key will be removed from the external source if it is
        /// not in the dictionary.
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <returns>True if key was removed, false if not</returns>
        public bool Remove(TKey key)
        {
            return this.InternalRemove(key);
        }

        /// <summary>
        /// Attempts to get a value given a key. If the key is not found in memory, it is
        /// possible for derived classes to search an external source to find the value. In cases where this
        /// is done, the newly found item may replace the leased used item if the dictionary is at max count.
        /// </summary>
        /// <param name="key">Key to find</param>
        /// <param name="value">Found value (default of TValue if not found)</param>
        /// <returns>True if found, false if not</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.TryGetValueRef(ref key, out value);
        }

        /// <summary>
        /// Attempts to get a value given a key. If the key is not found in memory, it is
        /// possible for derived classes to search an external source to find the value. In cases where this
        /// is done, the newly found item may replace the leased used item if the dictionary is at max count.
        /// </summary>
        /// <param name="key">Key to find (receives the found key)</param>
        /// <param name="value">Found value (default of TValue if not found)</param>
        /// <returns>True if found, false if not</returns>
        public bool TryGetValueRef(ref TKey key, out TValue value)
        {
            if (this.dictionary.TryGetValue(key, out var node))
            {
				this.MoveToFront(node);
                value = node.Value.Value;
                key = node.Value.Key;
                return true;
            }

            if (this.OnGetExternalKeyValue(ref key, out value))
            {
				this.Add(key, value);
                return true;
            }
			
            value = default;
            return false;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="key">N/A</param>
        /// <returns>N/A</returns>
        public TValue this[TKey key]
        {
            get => throw new NotSupportedException("Use TryGetValue instead");
            set => throw new NotSupportedException("Use Add instead");
        }
		
        /// <summary>
        /// Gets all the keys that are in memory
        /// </summary>
        public ICollection<TKey> Keys => this.dictionary.Keys;

        /// <summary>
        /// Gets all of the values that are in memory, external values are not returned
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>(this.dictionary.Values.Count);
                foreach (var node in this.dictionary.Values)
                {
                    values.Add(node.Value.Value);
                }
				
                return values;
            }
        }
		
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Adds an item with the key and value
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <exception cref="ArgumentException">An item with the key already exists</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears the dictionary of all items and priority information
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
            priorityList.Clear();
        }

        /// <summary>
        /// Checks to see if an item exists in the dictionary
        /// </summary>
        /// <param name="item">Item to check for</param>
        /// <returns>True if key of item exists in dictionary, false if not</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        /// <summary>
        /// Copies all items from the in memory dictionary to an array
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="arrayIndex">Start index to copy into array</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var keyValue in dictionary)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(keyValue.Key, keyValue.Value.Value.Value);
            }
        }

        /// <summary>
        /// Number of items in the in memory dictionary
        /// </summary>
        public int Count => dictionary.Count;

        /// <summary>
        /// Always false
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Removes an item from the in memory dictionary
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True if an item was removed, false if not</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Enumerates all key value pairs in the dictionary, external values are not enumerated
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var node in dictionary.Values)
            {
                yield return node.Value;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Enumerates all key value pairs in the dictionary, external values are not enumerated
        /// </summary>
        /// <returns>Enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
		
		#region Disposing

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.dictionary = null;
				this.priorityList = null;
				this.maxCount = 0;
			}
		}

		#endregion
    }
}