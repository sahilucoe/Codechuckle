namespace SimpleCache.Cache
{
    public interface ICache<TKey, TValue>
    {
        void Add(TKey key, TValue value, TimeSpan expiration);
        TValue Get(TKey key);
        void Remove(TKey key);
        int Size();
        void StopCleanupTimer();
    }
}