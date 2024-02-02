namespace GardenLogWeb.Shared.Services;

public interface ICacheService
{
    TItem Set<TItem>(object key, TItem value);
    TItem Set<TItem>(object key, TItem value, DateTime expireAfter);
    bool TryGetValue<TItem>(object key, out TItem? value);
    bool Remove(object key);
}

public class CacheService : ICacheService
{
    private readonly Dictionary<object, CacheItem> _cache = new();

    public bool TryGetValue<TItem>(object key, out TItem? value)
    {
        if (_cache.TryGetValue(key, out var cacheItem))
        {
            if (cacheItem.ExporeAfter.HasValue && cacheItem.ExporeAfter.Value < DateTime.Now)
            {
                _cache.Remove(key);
            }
            else
            {
                value = (TItem)cacheItem.Item;
                return true;
            }
        }

        value = default;
        return false;

    }
    public TItem Set<TItem>(object key, TItem value)
    {
        if (value == null) return value;
        if (_cache.ContainsKey(key))
        {
            _cache[key] = new CacheItem(value, null);
        }
        else
        {
            _cache.Add(key, new CacheItem(value, null));
        }
        return value;
    }

    public TItem Set<TItem>(object key, TItem value, DateTime expireAfter)
    {
        if (value == null) return value;
        if (_cache.ContainsKey(key))
        {
            _cache[key] = new CacheItem(value, expireAfter);
        }
        else
        {
            _cache.Add(key, new CacheItem(value, expireAfter));
        }
        return value;
    }

    public bool Remove(object key)
    {
        return _cache.Remove(key);
    }

    private record CacheItem(object Item, DateTime? ExporeAfter);

}
