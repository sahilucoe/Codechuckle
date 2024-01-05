using System.Collections.Concurrent;
using Timer = System.Timers.Timer;

namespace SimpleCache.Cache
{
    public class LRUCache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull 
    {
        private readonly int capacity;
        private readonly ConcurrentDictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>> cacheMap;
        private readonly LinkedList<CacheItem<TKey, TValue>> lruList;
        private readonly Timer cleanupTimer;

        public LRUCache(int capacity, TimeSpan cleanupInterval)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            this.capacity = capacity;
            cacheMap = new ConcurrentDictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>>();
            lruList = new LinkedList<CacheItem<TKey, TValue>>();

            // Start a timer to clean up expired items
            cleanupTimer = new Timer(cleanupInterval.TotalMilliseconds);
            cleanupTimer.Elapsed += CleanupExpiredItems;
            cleanupTimer.AutoReset = true;
            cleanupTimer.Start();
        }

        public TValue? Get(TKey key)
        {
            if (cacheMap.TryGetValue(key, out var item))
            {
                // Check for expiration
                if (item.Value.IsExpired())
                {
                    Remove(key);
                    return default;
                }

                // Move the accessed item to the front of the LRU list
                lruList.Remove(item.Value);
                lruList.AddFirst(item.Value);
                return item.Value.Value;
            }

            return default;
        }

        public void Add(TKey key, TValue value, TimeSpan expiration)
        {
            if (cacheMap.Count >= capacity)
            {
                Evict();
            }

            var newItem = new CacheItem<TKey, TValue>(key, value, expiration);
            var node = new LinkedListNode<CacheItem<TKey, TValue>>(newItem);
            lruList.AddFirst(node);
            cacheMap[key] = node;
        }

        private void Evict()
        {
            var itemToRemove = lruList.Last.Value;
            Remove(itemToRemove.Key);
        }

        public void Remove(TKey key)
        {
            if (cacheMap.TryRemove(key, out var item))
            {
                lruList.Remove(item);
            }
        }

        public void StopCleanupTimer()
        {
            cleanupTimer.Enabled = false;
        }

        private void CleanupExpiredItems(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var expiredItems = cacheMap.Values.Where(item => item.Value.IsExpired()).ToList();

            foreach (var expiredItem in expiredItems)
            {
                Remove(expiredItem.Value.Key);
            }
        }

        public int Size()
        {
            return cacheMap.Count;
        }
    }
}