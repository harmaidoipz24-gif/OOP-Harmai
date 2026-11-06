using System.Globalization;
using System.Text;

namespace IndependentWork15;


public static class CollectionUtils
{
    // Перетворює кожен елемент за допомогою mapper (аналог LINQ Select)
    public static IEnumerable<TResult> Map<TSource, TResult>(
        IEnumerable<TSource> collection,
        Func<TSource, TResult> mapper)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(mapper);

        List<TResult> result = new List<TResult>();
        foreach (TSource item in collection)
        {
            result.Add(mapper(item));
        }
        return result;
    }

    // Повертає елемент з максимальним значенням ключа (аналог LINQ MaxBy з .NET 6)
    public static T MaxBy<T, TKey>(
        IEnumerable<T> collection,
        Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(keySelector);

        using IEnumerator<T> enumerator = collection.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("Колекція порожня — максимального елемента немає.");

        T maxItem = enumerator.Current;
        TKey maxKey = keySelector(maxItem);

        while (enumerator.MoveNext())
        {
            T item = enumerator.Current;
            TKey key = keySelector(item);
            if (key.CompareTo(maxKey) > 0)
            {
                maxKey = key;
                maxItem = item;
            }
        }

        return maxItem;
    }

    // Повертає елементи, що задовольняють умову (аналог LINQ Where)
    public static IEnumerable<T> Filter<T>(
        IEnumerable<T> collection,
        Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(predicate);

        List<T> result = new List<T>();
        foreach (T item in collection)
        {
            if (predicate(item))
                result.Add(item);
        }
        return result;
    }

    // Виконує дію для кожного елемента (аналог List<T>.ForEach, але для будь-якого IEnumerable<T>)
    public static void ForEach<T>(
        IEnumerable<T> collection,
        Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(action);

        foreach (T item in collection)
        {
            action(item);
        }
    }
}

public class Product
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

        ProductsDemo();
        NumbersDemo();
        WordsDemo();
        DelegateVariablesDemo();
    }

    // Колекція 1: List<Product>
    private static void ProductsDemo()
    {
        Console.WriteLine("Колекція 1: List<Product>");

        List<Product> products = new List<Product>
        {
            new Product { Name = "Laptop", Price = 25000, Category = "Electronics" },
            new Product { Name = "Mouse", Price = 500, Category = "Accessories" },
            new Product { Name = "Keyboard", Price = 1500, Category = "Accessories" },
            new Product { Name = "Monitor", Price = 7200, Category = "Electronics" },
            new Product { Name = "USB cable", Price = 150, Category = "Accessories" }
        };

        IEnumerable<string> names = CollectionUtils.Map(products, p => p.Name);
        Console.WriteLine("Map (назви): " + string.Join(", ", names));

        Product mostExpensive = CollectionUtils.MaxBy(products, p => p.Price);
        Console.WriteLine($"MaxBy (найдорожчий): {mostExpensive.Name} - {mostExpensive.Price} грн");

        Product longestName = CollectionUtils.MaxBy(products, p => p.Name.Length);
        Console.WriteLine($"MaxBy (найдовша назва): {longestName.Name}");

        IEnumerable<Product> expensive = CollectionUtils.Filter(products, p => p.Price > 1000);
        Console.WriteLine("Filter (дорожчі за 1000 грн):");
        CollectionUtils.ForEach(expensive, p => Console.WriteLine($"   - {p.Name}"));

        Console.WriteLine("ForEach (усі товари зі знижкою 10%):");
        CollectionUtils.ForEach(products, p => Console.WriteLine($"   {p.Name}: {p.Price} грн -> {p.Price * 0.9m:F2} грн"));
        Console.WriteLine();
    }

    // Колекція 2: List<int>
    private static void NumbersDemo()
    {
        Console.WriteLine("Колекція 2: List<int>");

        List<int> numbers = new List<int> { 7, -3, 12, 0, 25, -18, 4, 9 };

        IEnumerable<int> squares = CollectionUtils.Map(numbers, n => n * n);
        Console.WriteLine("Map (квадрати): " + string.Join(", ", squares));

        IEnumerable<string> parity = CollectionUtils.Map(numbers, n => n % 2 == 0 ? "парне" : "непарне");
        Console.WriteLine("Map (int -> string): " + string.Join(", ", parity));

        int maxAbs = CollectionUtils.MaxBy(numbers, n => Math.Abs(n));
        Console.WriteLine($"MaxBy (найбільше за модулем): {maxAbs}");

        IEnumerable<int> positiveEven = CollectionUtils.Filter(numbers, n => n > 0 && n % 2 == 0);
        Console.WriteLine("Filter (додатні парні): " + string.Join(", ", positiveEven));

        int sum = 0;
        CollectionUtils.ForEach(numbers, n => sum += n); // лямбда захоплює зовнішню змінну (замикання)
        Console.WriteLine($"ForEach (сума через замикання): {sum}");
        Console.WriteLine();
    }

    // Колекція 3: string[]
    private static void WordsDemo()
    {
        Console.WriteLine("Колекція 3: string[]");

        string[] words = { "delegate", "lambda", "generic", "LINQ", "predicate", "action", "func" };

        IEnumerable<string> upper = CollectionUtils.Map(words, w => w.ToUpper());
        Console.WriteLine("Map (верхній регістр): " + string.Join(", ", upper));

        IEnumerable<int> lengths = CollectionUtils.Map(words, w => w.Length);
        Console.WriteLine("Map (довжини): " + string.Join(", ", lengths));

        string longest = CollectionUtils.MaxBy(words, w => w.Length);
        Console.WriteLine($"MaxBy (найдовше слово): {longest}");

        string lastAlphabetically = CollectionUtils.MaxBy(words, w => w.ToLower());
        Console.WriteLine($"MaxBy (останнє за алфавітом): {lastAlphabetically}");

        IEnumerable<string> shortWords = CollectionUtils.Filter(words, w => w.Length <= 6);
        Console.WriteLine("Filter (не довші за 6 літер): " + string.Join(", ", shortWords));

        Console.Write("ForEach (перші літери): ");
        CollectionUtils.ForEach(words, w => Console.Write(w[0]));
        Console.WriteLine();
        Console.WriteLine();
    }

    // Делегати як змінні: одну й ту саму поведінку можна зберегти і використати повторно
    private static void DelegateVariablesDemo()
    {
        Console.WriteLine("Делегати як змінні");

        Predicate<int> isEven = n => n % 2 == 0;
        Func<int, string> describe = n => $"[{n}]";
        Action<string> print = s => Console.Write(s + " ");

        List<int> range = Enumerable.Range(1, 10).ToList();

        IEnumerable<int> evens = CollectionUtils.Filter(range, isEven);
        IEnumerable<string> described = CollectionUtils.Map(evens, describe);
        CollectionUtils.ForEach(described, print);
        Console.WriteLine();

        // Метод замість лямбди (method group)
        IEnumerable<string> roots = CollectionUtils.Map(new[] { 4.0, 9.0, 2.0 }, FormatSqrt);
        Console.WriteLine("Map з іменованим методом: " + string.Join(", ", roots));
    }

    private static string FormatSqrt(double x) => $"√{x} = {Math.Sqrt(x):F2}";
}
