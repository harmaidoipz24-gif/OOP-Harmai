using System.Collections;
using System.Text;

namespace IndependentWork5;

public class Product
{
    public string Name { get; }
    public decimal Price { get; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public override string ToString() => $"Product: {Name}, ціна {Price:F2} грн";
}

public class Customer
{
    public string FullName { get; }
    public string Email { get; }

    public Customer(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
    }

    public override string ToString() => $"Customer: {FullName} <{Email}>";
}

public class Order
{
    public int Number { get; }
    public Customer Customer { get; }
    public List<Product> Items { get; }

    public Order(int number, Customer customer, List<Product> items)
    {
        Number = number;
        Customer = customer;
        Items = items;
    }

    public decimal Total => Items.Sum(p => p.Price);

    public override string ToString() =>
        $"Order #{Number}: {Customer.FullName}, товарів {Items.Count}, сума {Total:F2} грн";
}

// Клас без перевизначення — для порівняння
public class Warehouse
{
    public string City { get; } = "Рівне";
}

// Сценарій 2: IEnumerable<string>

public class MyCustomCollection : IEnumerable<string>
{
    private readonly List<string> _items = new List<string>();

    public void Add(string item) => _items.Add(item);

    // Узагальнена реалізація — її використовує foreach
    public IEnumerator<string> GetEnumerator()
    {
        foreach (string item in _items)
        {
            yield return item;
        }
    }

    // Неузагальнена реалізація з IEnumerable (явна реалізація інтерфейсу)
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// Програма

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        RunObjectScenario();
        RunEnumerableScenario();
        RunStreamScenario();
    }

    private static void RunObjectScenario()
    {
        Console.WriteLine("1. Поліморфізм через System.Object");

        Customer customer = new Customer("Денис Гармай", "denys@example.com");
        Product laptop = new Product("Laptop", 35000m);
        Product mouse = new Product("Mouse", 800m);
        Order order = new Order(1001, customer, new List<Product> { laptop, mouse });

        List<object> objects = new List<object> { laptop, customer, order, new Warehouse(), 42, DateOnly.FromDateTime(new DateTime(2026, 10, 22)) };

        foreach (object obj in objects)
        {
            // Статичний тип змінної — object, але викликається ToString() фактичного типу
            Console.WriteLine($"{obj.GetType().Name,-10} -> {obj}");
        }

        // Висновок: змінна має тип object, але Console.WriteLine викликає віртуальний
        // ToString(), і CLR під час виконання обирає реалізацію фактичного класу
        // (Product, Customer, Order). Warehouse не перевизначив метод, тому спрацювала
        // базова версія Object.ToString(), яка повертає лише повну назву типу.
        Console.WriteLine();
    }

    private static void RunEnumerableScenario()
    {
        Console.WriteLine("2. Поліморфізм через IEnumerable<string>");

        MyCustomCollection courses = new MyCustomCollection();
        courses.Add("ООП");
        courses.Add("Бази даних");
        courses.Add("Розробка ігор у Unity");

        Console.WriteLine("foreach по MyCustomCollection:");
        foreach (string course in courses)
        {
            Console.WriteLine($"  - {course}");
        }

        // Один і той самий метод працює з будь-якою колекцією, що реалізує IEnumerable<string>
        string[] array = { "C#", "Java" };
        List<string> list = new List<string> { "Python", "Go", "Rust" };

        PrintAll("MyCustomCollection", courses);
        PrintAll("string[]", array);
        PrintAll("List<string>", list);

        // LINQ теж працює, бо його методи розширення приймають IEnumerable<T>
        Console.WriteLine($"LINQ: курсів, довших за 5 символів: {courses.Count(c => c.Length > 5)}");

        // Висновок: foreach не знає конкретного типу колекції — він викликає GetEnumerator()
        // з інтерфейсу. Тому MyCustomCollection, масив і List<T> перебираються однаково.
        Console.WriteLine();
    }

    private static void PrintAll(string label, IEnumerable<string> items)
    {
        Console.WriteLine($"{label}: {string.Join(", ", items)}");
    }

    private static void RunStreamScenario()
    {
        Console.WriteLine("3. Поліморфізм через абстрактний клас Stream");

        // Тестовий файл створюємо в коді, щоб проєкт не залежав від зовнішніх файлів
        string filePath = Path.Combine(AppContext.BaseDirectory, "test.txt");
        File.WriteAllText(filePath, "Hello from FileStream!", Encoding.UTF8);

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            ProcessStream(fileStream);
        }

        byte[] memoryData = Encoding.UTF8.GetBytes("Hello from MemoryStream!");
        using (MemoryStream memoryStream = new MemoryStream(memoryData))
        {
            ProcessStream(memoryStream);
        }

        File.Delete(filePath);

        // Висновок: ProcessStream знає лише про абстрактний Stream. Виклики Read, Length,
        // CanSeek є віртуальними/абстрактними, тому для FileStream дані читаються з диска,
        // а для MemoryStream — з масиву в пам'яті, хоча код методу той самий.
    }

    // Метод працює з будь-яким нащадком Stream
    private static void ProcessStream(Stream stream)
    {
        Console.WriteLine($"Тип потоку: {stream.GetType().Name}");
        Console.WriteLine($"  CanRead: {stream.CanRead}, CanSeek: {stream.CanSeek}, довжина: {stream.Length} байт");

        byte[] buffer = new byte[10];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        Console.WriteLine($"  Прочитано {bytesRead} байт: {BitConverter.ToString(buffer, 0, bytesRead)}");
        Console.WriteLine($"  Як текст: \"{Encoding.UTF8.GetString(buffer, 0, bytesRead)}\"");
    }
}
