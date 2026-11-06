using IndependentWork13;

namespace IndependentWork13.Tests;

public class CacheTests
{
    // Конструктор

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_NonPositiveCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cache<string, int>(capacity));
    }

    [Fact]
    public void NewCache_IsEmpty()
    {
        Cache<string, int> cache = new Cache<string, int>(3);

        Assert.Equal(0, cache.Count);
    }

    // Add

    [Fact]
    public void Add_NewItem_ItemIsStored()
    {
        Cache<string, int> cache = new Cache<string, int>(3);

        cache.Add("a", 1);

        Assert.True(cache.ContainsKey("a"));
        Assert.Equal(1, cache.Get("a"));
    }

    [Fact]
    public void Add_ExistingKey_UpdatesValueWithoutIncreasingCount()
    {
        Cache<string, int> cache = new Cache<string, int>(3);
        cache.Add("a", 1);

        cache.Add("a", 100);

        Assert.Equal(1, cache.Count);
        Assert.Equal(100, cache.Get("a"));
    }

    // Get

    [Fact]
    public void Get_ExistingKey_ReturnsCorrectValue()
    {
        Cache<int, string> cache = new Cache<int, string>(3);
        cache.Add(1, "one");
        cache.Add(2, "two");

        Assert.Equal("one", cache.Get(1));
        Assert.Equal("two", cache.Get(2));
    }

    [Fact]
    public void Get_MissingKey_ThrowsKeyNotFoundException()
    {
        Cache<string, int> cache = new Cache<string, int>(3);
        cache.Add("a", 1);

        Assert.Throws<KeyNotFoundException>(() => cache.Get("missing"));
    }

    [Fact]
    public void Get_EvictedKey_ThrowsKeyNotFoundException()
    {
        Cache<string, int> cache = new Cache<string, int>(1);
        cache.Add("a", 1);
        cache.Add("b", 2);

        Assert.Throws<KeyNotFoundException>(() => cache.Get("a"));
    }

    // Переповнення кешу (FIFO)

    [Fact]
    public void Add_WhenFull_RemovesOldestItem()
    {
        Cache<string, int> cache = new Cache<string, int>(3);
        cache.Add("a", 1);
        cache.Add("b", 2);
        cache.Add("c", 3);

        cache.Add("d", 4);

        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
        Assert.True(cache.ContainsKey("c"));
        Assert.True(cache.ContainsKey("d"));
    }

    [Fact]
    public void Add_SeveralOverflows_RemovesItemsInInsertionOrder()
    {
        Cache<int, string> cache = new Cache<int, string>(2);
        cache.Add(1, "one");
        cache.Add(2, "two");

        cache.Add(3, "three"); // витісняє 1
        cache.Add(4, "four");  // витісняє 2

        Assert.Equal(new[] { 3, 4 }, cache.KeysInOrder);
    }

    [Fact]
    public void Add_GetDoesNotChangeFifoOrder()
    {
        Cache<string, int> cache = new Cache<string, int>(2);
        cache.Add("a", 1);
        cache.Add("b", 2);

        cache.Get("a"); // у FIFO (на відміну від LRU) звернення не "освіжає" елемент
        cache.Add("c", 3);

        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
    }

    [Fact]
    public void Add_UpdatingExistingKey_DoesNotEvictAnything()
    {
        Cache<string, int> cache = new Cache<string, int>(2);
        cache.Add("a", 1);
        cache.Add("b", 2);

        cache.Add("b", 20);

        Assert.True(cache.ContainsKey("a"));
        Assert.Equal(2, cache.Count);
    }

    // ContainsKey

    [Fact]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        Cache<string, int> cache = new Cache<string, int>(2);
        cache.Add("a", 1);

        Assert.True(cache.ContainsKey("a"));
    }

    [Fact]
    public void ContainsKey_MissingKey_ReturnsFalse()
    {
        Cache<string, int> cache = new Cache<string, int>(2);
        cache.Add("a", 1);

        Assert.False(cache.ContainsKey("z"));
    }

    // Count

    [Fact]
    public void Count_IncreasesWithEachNewItem()
    {
        Cache<string, int> cache = new Cache<string, int>(5);

        cache.Add("a", 1);
        Assert.Equal(1, cache.Count);

        cache.Add("b", 2);
        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void Count_NeverExceedsCapacity()
    {
        Cache<int, int> cache = new Cache<int, int>(3);

        for (int i = 0; i < 10; i++)
        {
            cache.Add(i, i * i);
        }

        Assert.Equal(3, cache.Count);
    }
}
