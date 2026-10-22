using System.Globalization;
using System.Text;

namespace IndependentWork2;

// Самостійна робота №2. Перевантаження конструкторів: приклади та сценарії.

public class Product
{
    // Приватні поля
    private readonly int _id;
    private readonly string _name;
    private readonly decimal _price;
    private readonly string _category;
    private readonly int _stockCount;

    // Публічні read-only властивості
    public int Id { get { return _id; } }
    public string Name { get { return _name; } }
    public decimal Price { get { return _price; } }
    public string Category { get { return _category; } }
    public int StockCount { get { return _stockCount; } }

    // Конструктор 1 (основний): приймає всі п'ять параметрів
    public Product(int id, string name, decimal price, string category, int stockCount)
    {
        _id = id;
        _name = name;
        _price = price;
        _category = category;
        _stockCount = stockCount;
    }

    // Конструктор 2 (швидке додавання): делегує основному через this(...)
    public Product(int id, string name, decimal price)
        : this(id, name, price, "Uncategorized", 0)
    {
    }

    // Конструктор 3 (копіювання): теж делегує основному через this(...)
    public Product(Product other)
        : this(other.Id, other.Name, other.Price, other.Category, other.StockCount)
    {
    }

    public override string ToString()
    {
        return $"ID: {Id}, Name: {Name}, Price: {Price:C}, Category: {Category}, Stock: {StockCount}";
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        // Українська культура, щоб :C виводив ціну у гривнях (35 000,00 ₴)
        CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

        Console.WriteLine("Створення товарів");

        Product laptop = new Product(101, "Laptop", 35000m, "Electronics", 15);
        Product mouse = new Product(102, "Mouse", 800m);
        Product laptopCopy = new Product(laptop);

        Console.WriteLine($"Товар 1 (основний конструктор): {laptop}");
        Console.WriteLine($"Товар 2 (скорочений конструктор): {mouse}");
        Console.WriteLine($"Товар 3 (конструктор копіювання): {laptopCopy}");

        Console.WriteLine();
        Console.WriteLine($"Копія — це окремий об'єкт: {!ReferenceEquals(laptop, laptopCopy)}");
    }
}

/*
 Відповіді на контрольні запитання

 1. Навіщо перевантаження конструкторів у класі Product?
    Товар додається з різним рівнем деталізації: інколи відомі лише id, назва і ціна,
    інколи — повний набір даних. Кілька конструкторів дозволяють створити коректний
    об'єкт у кожному сценарії без "порожніх" або вигаданих аргументів у клієнтському коді.

 2. Чому конструктор копіювання зручно реалізувати через this(...)?
    Уся логіка ініціалізації зосереджена в одному (основному) конструкторі. Якщо в нього
    додати перевірку або нове поле, зміни автоматично діятимуть і для копіювання, — немає
    дублювання присвоєнь.

 3. Які переваги read-only властивостей?
    Стан об'єкта фіксується в момент створення і не може бути випадково змінений ззовні.
    Такий об'єкт безпечно передавати в різні частини програми та між потоками, його
    легше тестувати, а копія гарантовано не зміниться через оригінал.

 4. Коли перевизначення ToString() спрощує налагодження?
    Коли об'єкт виводиться в консоль, лог або переглядається в налагоджувачі: замість
    назви типу "IndependentWork2.Product" одразу видно всі значення полів.
*/
