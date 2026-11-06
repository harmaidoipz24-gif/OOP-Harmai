using System.Text;

namespace IndependentWork13;

public class Student
{
    public int Id { get; }
    public string Name { get; }
    public double AverageGrade { get; }

    public Student(int id, string name, double averageGrade)
    {
        Id = id;
        Name = name;
        AverageGrade = averageGrade;
    }

    public override string ToString() => $"{Name} (середній бал {AverageGrade:F1})";
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        StringCacheDemo();
        Console.WriteLine();
        ObjectCacheDemo();
    }

    private static void StringCacheDemo()
    {
        Console.WriteLine("Cache<string, string> з ємністю 3");

        Cache<string, string> translations = new Cache<string, string>(3);
        translations.Add("cat", "кіт");
        translations.Add("dog", "пес");
        translations.Add("bird", "птах");
        PrintState(translations);

        Console.WriteLine($"Get(\"dog\") = {translations.Get("dog")}");

        Console.WriteLine("Додаємо \"fish\" — кеш повний, має видалитися найстаріший ключ \"cat\"");
        translations.Add("fish", "риба");
        PrintState(translations);
        Console.WriteLine($"ContainsKey(\"cat\") = {translations.ContainsKey("cat")}");
        Console.WriteLine($"ContainsKey(\"fish\") = {translations.ContainsKey("fish")}");

        Console.WriteLine("Оновлюємо значення \"dog\" — порядок FIFO не змінюється");
        translations.Add("dog", "собака");
        translations.Add("horse", "кінь"); // тепер витісняється "dog", бо він найстаріший
        PrintState(translations);

        try
        {
            translations.Get("cat");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"KeyNotFoundException: {ex.Message}");
        }
    }

    private static void ObjectCacheDemo()
    {
        Console.WriteLine("Cache<int, Student> з ємністю 2");

        Cache<int, Student> students = new Cache<int, Student>(2);
        students.Add(1, new Student(1, "Денис Гармай", 4.6));
        students.Add(2, new Student(2, "Олена Коваль", 4.9));
        Console.WriteLine($"Get(1) = {students.Get(1)}");

        students.Add(3, new Student(3, "Максим Бондар", 4.2));
        Console.WriteLine($"Після додавання третього студента Count = {students.Count}");
        Console.WriteLine($"Студент 1 у кеші: {students.ContainsKey(1)}");
        Console.WriteLine($"Get(3) = {students.Get(3)}");
    }

    private static void PrintState<TKey, TValue>(Cache<TKey, TValue> cache) where TKey : notnull
    {
        IEnumerable<string> items = cache.KeysInOrder.Select(k => $"{k}={cache.Get(k)}");
        Console.WriteLine($"   Count = {cache.Count}/{cache.Capacity}: [{string.Join(", ", items)}]");
    }
}
