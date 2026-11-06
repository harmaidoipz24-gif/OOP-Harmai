namespace IndependentWork13;

/// <summary>
/// Узагальнений кеш з обмеженою ємністю та політикою витіснення FIFO:
/// коли кеш заповнений, видаляється елемент, який було додано найраніше.
/// </summary>
public class Cache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cache;
    private readonly Queue<TKey> _keysOrder;

    public Cache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Ємність кешу має бути більшою за нуль.");

        _capacity = capacity;
        _cache = new Dictionary<TKey, TValue>(capacity);
        _keysOrder = new Queue<TKey>(capacity);
    }

    public int Count => _cache.Count;

    public int Capacity => _capacity;

    /// <summary>
    /// Додає елемент. Якщо ключ уже є, оновлює значення без зміни порядку
    /// (для FIFO важливий момент першого додавання). Якщо кеш повний, видаляє найстаріший елемент.
    /// </summary>
    public void Add(TKey key, TValue value)
    {
        if (_cache.ContainsKey(key))
        {
            _cache[key] = value;
            return;
        }

        if (_cache.Count >= _capacity)
        {
            TKey oldestKey = _keysOrder.Dequeue();
            _cache.Remove(oldestKey);
        }

        _cache.Add(key, value);
        _keysOrder.Enqueue(key);
    }

    /// <summary>Повертає значення за ключем або викидає KeyNotFoundException.</summary>
    public TValue Get(TKey key)
    {
        if (_cache.TryGetValue(key, out TValue? value))
            return value;

        throw new KeyNotFoundException($"Ключ '{key}' відсутній у кеші.");
    }

    public bool ContainsKey(TKey key) => _cache.ContainsKey(key);

    /// <summary>Ключі в порядку додавання: від найстарішого до найновішого.</summary>
    public IEnumerable<TKey> KeysInOrder => _keysOrder;
}
