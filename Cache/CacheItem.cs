namespace SimpleCache.Cache
{
    public class CacheItem<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; }
        public DateTime Expiry { get; }

        public CacheItem(TKey key, TValue value, TimeSpan expiration)
        {
            Key = key;
            Value = value;
            Expiry = DateTime.Now.Add(expiration);
        }

        public bool IsExpired() => DateTime.Now > Expiry;
    }
}