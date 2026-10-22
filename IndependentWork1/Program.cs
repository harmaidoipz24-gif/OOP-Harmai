using System.Text;

namespace IndependentWork1;

// Самостійна робота №1. Базовий синтаксис C# і оголошення класів.
// Класи: Thermostat (термостат), ParkingLot (автостоянка), LibraryCard (читацький квиток).

/// <summary>Термостат: зберігає поточну та цільову температуру і вирішує, чи вмикати обігрів.</summary>
public class Thermostat
{
    // Приватні поля
    private string _room;
    private double _currentTemperature;
    private double _targetTemperature;

    // Властивість лише для читання
    public string Room
    {
        get { return _room; }
    }

    // Властивість з get і set (з перевіркою допустимого діапазону)
    public double TargetTemperature
    {
        get { return _targetTemperature; }
        set
        {
            if (value < 5 || value > 30)
                throw new ArgumentOutOfRangeException(nameof(value), "Цільова температура має бути від 5 до 30 °C.");
            _targetTemperature = value;
        }
    }

    public double CurrentTemperature
    {
        get { return _currentTemperature; }
        set { _currentTemperature = value; }
    }

    // Конструктор
    public Thermostat(string room, double currentTemperature, double targetTemperature)
    {
        _room = room;
        _currentTemperature = currentTemperature;
        TargetTemperature = targetTemperature; // через властивість, щоб спрацювала перевірка
    }

    // Метод з перевіркою стану
    public bool NeedsHeating()
    {
        return _currentTemperature < _targetTemperature;
    }

    // Метод з простим обчисленням
    public double GetDifference()
    {
        return Math.Round(_targetTemperature - _currentTemperature, 1);
    }
}

/// <summary>Автостоянка: кількість місць, зайнятість і розрахунок вартості паркування.</summary>
public class ParkingLot
{
    private string _address;
    private int _capacity;
    private int _occupied;
    private decimal _pricePerHour;

    public string Address
    {
        get { return _address; }
        set { _address = value; }
    }

    // read-only
    public int Capacity
    {
        get { return _capacity; }
    }

    public int Occupied
    {
        get { return _occupied; }
    }

    public int FreeSpaces
    {
        get { return _capacity - _occupied; }
    }

    public ParkingLot(string address, int capacity, decimal pricePerHour)
    {
        _address = address;
        _capacity = capacity;
        _pricePerHour = pricePerHour;
        _occupied = 0;
    }

    // Заїзд автомобіля: повертає false, якщо місць немає
    public bool ParkCar()
    {
        if (_occupied >= _capacity)
            return false;
        _occupied++;
        return true;
    }

    public void LeaveCar()
    {
        if (_occupied > 0)
            _occupied--;
    }

    // Обчислення вартості: кожна розпочата година оплачується повністю
    public decimal CalculateFee(int minutes)
    {
        int hours = (int)Math.Ceiling(minutes / 60.0);
        return hours * _pricePerHour;
    }
}

/// <summary>Читацький квиток: власник, ліміт книг і список виданих книг.</summary>
public class LibraryCard
{
    private string _ownerName;
    private int _maxBooks;
    private readonly List<string> _borrowedBooks;

    public string OwnerName
    {
        get { return _ownerName; }
        set { _ownerName = value; }
    }

    public int MaxBooks
    {
        get { return _maxBooks; }
    }

    public int BorrowedCount
    {
        get { return _borrowedBooks.Count; }
    }

    public LibraryCard(string ownerName, int maxBooks)
    {
        _ownerName = ownerName;
        _maxBooks = maxBooks;
        _borrowedBooks = new List<string>();
    }

    public bool BorrowBook(string title)
    {
        if (_borrowedBooks.Count >= _maxBooks)
            return false;
        _borrowedBooks.Add(title);
        return true;
    }

    public bool ReturnBook(string title)
    {
        return _borrowedBooks.Remove(title);
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Читач: {_ownerName}, книг на руках: {_borrowedBooks.Count}/{_maxBooks}");
        foreach (string book in _borrowedBooks)
            Console.WriteLine($"   - {book}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Thermostat
        Console.WriteLine("Thermostat");
        Thermostat thermostat = new Thermostat("Вітальня", 18.5, 22);
        Console.WriteLine($"Кімната: {thermostat.Room}, зараз {thermostat.CurrentTemperature} °C, ціль {thermostat.TargetTemperature} °C");
        Console.WriteLine($"Потрібен обігрів: {(thermostat.NeedsHeating() ? "так" : "ні")}, різниця: {thermostat.GetDifference()} °C");
        thermostat.CurrentTemperature = 23;
        Console.WriteLine($"Після нагріву до {thermostat.CurrentTemperature} °C обігрів потрібен: {(thermostat.NeedsHeating() ? "так" : "ні")}");
        try
        {
            thermostat.TargetTemperature = 45;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Помилка: {ex.Message.Split(Environment.NewLine)[0]}");
        }

        // ParkingLot
        Console.WriteLine();
        Console.WriteLine("ParkingLot");
        ParkingLot parking = new ParkingLot("вул. Соборна, 10", 2, 25m);
        Console.WriteLine($"Стоянка: {parking.Address}, місць: {parking.Capacity}");
        Console.WriteLine($"Авто 1 заїхало: {parking.ParkCar()}");
        Console.WriteLine($"Авто 2 заїхало: {parking.ParkCar()}");
        Console.WriteLine($"Авто 3 заїхало: {parking.ParkCar()} (місць немає)");
        parking.LeaveCar();
        Console.WriteLine($"Вільних місць після виїзду: {parking.FreeSpaces}");
        Console.WriteLine($"Вартість 135 хв паркування: {parking.CalculateFee(135):F2} грн");

        // LibraryCard
        Console.WriteLine();
        Console.WriteLine("LibraryCard");
        LibraryCard card = new LibraryCard("Денис Гармай", 2);
        card.BorrowBook("CLR via C#");
        card.BorrowBook("Чистий код");
        bool third = card.BorrowBook("Патерни проєктування");
        Console.WriteLine($"Спроба взяти третю книгу: {(third ? "успішно" : "відмовлено, ліміт вичерпано")}");
        card.PrintInfo();
        card.ReturnBook("Чистий код");
        Console.WriteLine("Після повернення однієї книги:");
        card.PrintInfo();
    }
}
