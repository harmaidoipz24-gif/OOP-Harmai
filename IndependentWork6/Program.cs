using System.Text;
using System.Xml.Linq;

namespace IndependentWork6;

public interface INotifier
{
    string ChannelName { get; }
    void Send(string message);
}

// Додатковий інтерфейс: клас може реалізувати кілька інтерфейсів,
// але успадкувати лише один базовий клас.
public interface IDeliveryReport
{
    int SentCount { get; }
}

public class EmailNotifier : INotifier, IDeliveryReport
{
    private readonly string _address;

    public string ChannelName => "Email";
    public int SentCount { get; private set; }

    public EmailNotifier(string address)
    {
        _address = address;
    }

    public void Send(string message)
    {
        string subject = message.Length > 20 ? message[..20] + "..." : message;
        Console.WriteLine($"[Email] Кому: {_address}");
        Console.WriteLine($"        Тема: {subject}");
        Console.WriteLine($"        Текст: {message}");
        SentCount++;
    }
}

public class SmsNotifier : INotifier, IDeliveryReport
{
    private const int MaxLength = 70; // ліміт одного SMS кирилицею
    private readonly string _phoneNumber;

    public string ChannelName => "SMS";
    public int SentCount { get; private set; }

    public SmsNotifier(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
    }

    public void Send(string message)
    {
        // Довге повідомлення ділиться на кілька SMS
        int parts = (int)Math.Ceiling(message.Length / (double)MaxLength);
        for (int i = 0; i < parts; i++)
        {
            int start = i * MaxLength;
            string part = message.Substring(start, Math.Min(MaxLength, message.Length - start));
            Console.WriteLine($"[SMS]   {_phoneNumber} ({i + 1}/{parts}): {part}");
            SentCount++;
        }
    }
}

public class PushNotifier : INotifier
{
    private readonly string _deviceId;

    public string ChannelName => "Push";

    public PushNotifier(string deviceId)
    {
        _deviceId = deviceId;
    }

    public void Send(string message)
    {
        string preview = message.Length > 40 ? message[..40] + "..." : message;
        Console.WriteLine($"[Push]  Пристрій {_deviceId}: {preview}");
    }
}

// Клієнтський код залежить лише від абстракції INotifier (принцип DIP)
public class NotificationService
{
    private readonly List<INotifier> _notifiers;

    public NotificationService(List<INotifier> notifiers)
    {
        _notifiers = notifiers;
    }

    public void Broadcast(string message)
    {
        foreach (INotifier notifier in _notifiers)
        {
            notifier.Send(message);
        }
    }
}

// Сценарій 2: Обробка файлів
// Обрано АБСТРАКТНИЙ КЛАС: усі обробники мають однакові кроки
// (перевірка, відкриття, закриття) і спільний стан (шлях, кількість рядків).
// Різниться лише обробка вмісту — патерн "Шаблонний метод" (Template Method).

public abstract class FileProcessor
{
    private readonly string _filePath;
    private StreamReader? _reader;

    public string FileName => Path.GetFileName(_filePath);
    public int LinesRead { get; private set; }

    // Кожен нащадок зобов'язаний назвати свій формат
    public abstract string FormatName { get; }

    protected FileProcessor(string filePath)
    {
        _filePath = filePath;
    }

    // Шаблонний метод: послідовність кроків фіксована і не перевизначається
    public void Process()
    {
        Console.WriteLine($"[{FormatName}] Обробка файлу {FileName}");
        try
        {
            Open();
            string content = _reader!.ReadToEnd();
            LinesRead = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            ProcessContent(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Помилка обробки: {ex.Message}");
        }
        finally
        {
            Close(); // файл закривається навіть у разі помилки
        }
        Console.WriteLine();
    }

    // Спільні кроки — реалізовані один раз
    private void Open()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("Файл не знайдено", _filePath);

        _reader = new StreamReader(_filePath, Encoding.UTF8);
        Console.WriteLine("   Файл відкрито");
    }

    private void Close()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
            Console.WriteLine($"   Файл закрито (прочитано рядків: {LinesRead})");
        }
    }

    // Унікальна логіка — обов'язкова для кожного нащадка
    protected abstract void ProcessContent(string content);
}

public class TextFileProcessor : FileProcessor
{
    public override string FormatName => "TXT";

    public TextFileProcessor(string filePath) : base(filePath)
    {
    }

    protected override void ProcessContent(string content)
    {
        string[] words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"   Символів: {content.Length}, слів: {words.Length}");
        Console.WriteLine($"   Найдовше слово: {words.OrderByDescending(w => w.Length).First()}");
    }
}

public class XmlFileProcessor : FileProcessor
{
    public override string FormatName => "XML";

    public XmlFileProcessor(string filePath) : base(filePath)
    {
    }

    protected override void ProcessContent(string content)
    {
        XDocument document = XDocument.Parse(content);
        XElement root = document.Root!;
        Console.WriteLine($"   Кореневий елемент: <{root.Name}>, дочірніх елементів: {root.Elements().Count()}");
        foreach (XElement student in root.Elements("student"))
        {
            Console.WriteLine($"   - {student.Attribute("name")?.Value}, група {student.Attribute("group")?.Value}");
        }
    }
}

public class CsvFileProcessor : FileProcessor
{
    public override string FormatName => "CSV";

    public CsvFileProcessor(string filePath) : base(filePath)
    {
    }

    protected override void ProcessContent(string content)
    {
        string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string[] header = lines[0].Trim().Split(';');
        Console.WriteLine($"   Стовпці: {string.Join(", ", header)}");

        decimal total = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cells = lines[i].Trim().Split(';');
            total += decimal.Parse(cells[1], System.Globalization.CultureInfo.InvariantCulture);
        }
        Console.WriteLine($"   Записів: {lines.Length - 1}, сума стовпця \"{header[1]}\": {total:F2}");
    }
}

// Програма

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        RunNotificationScenario();
        RunFileScenario();
    }

    private static void RunNotificationScenario()
    {
        Console.WriteLine("Сценарій 1. Система сповіщень (інтерфейс INotifier)");
        Console.WriteLine();

        // Колекція базового типу — інтерфейсу
        List<INotifier> notifiers = new List<INotifier>
        {
            new EmailNotifier("denys@example.com"),
            new SmsNotifier("+380 67 123 45 67"),
            new PushNotifier("android-7f3a")
        };

        string message = "Нагадування: здати самостійну роботу №6 з ООП до п'ятниці. Посилання на GitHub-репозиторій надіслати викладачу.";

        foreach (INotifier notifier in notifiers)
        {
            Console.WriteLine($"Канал: {notifier.ChannelName}");
            notifier.Send(message);
            Console.WriteLine();
        }

        // Той самий набір каналів через сервіс, що знає лише про INotifier
        NotificationService service = new NotificationService(notifiers);
        Console.WriteLine("Розсилка через NotificationService:");
        service.Broadcast("Пара перенесена на 10:00");
        Console.WriteLine();

        // Перевірка додаткового інтерфейсу: його реалізують не всі канали
        Console.WriteLine("Звіт про доставку:");
        foreach (INotifier notifier in notifiers)
        {
            if (notifier is IDeliveryReport report)
                Console.WriteLine($"   {notifier.ChannelName}: надіслано {report.SentCount} повідомлень");
            else
                Console.WriteLine($"   {notifier.ChannelName}: звіт не підтримується");
        }
        Console.WriteLine();
    }

    private static void RunFileScenario()
    {
        Console.WriteLine("Сценарій 2. Обробка файлів (абстрактний клас FileProcessor)");
        Console.WriteLine();

        // Тестові файли створюються в коді, щоб проєкт не залежав від зовнішніх файлів
        string folder = Path.Combine(AppContext.BaseDirectory, "TestFiles");
        Directory.CreateDirectory(folder);

        string txtPath = Path.Combine(folder, "notes.txt");
        string xmlPath = Path.Combine(folder, "students.xml");
        string csvPath = Path.Combine(folder, "grades.csv");

        File.WriteAllText(txtPath,
            "Абстрактний клас задає спільні кроки алгоритму.\nІнтерфейс задає лише контракт поведінки.",
            Encoding.UTF8);
        File.WriteAllText(xmlPath,
            "<students>\n  <student name=\"Денис Гармай\" group=\"ІПЗ-3\" />\n  <student name=\"Олена Коваль\" group=\"ІПЗ-3\" />\n</students>",
            Encoding.UTF8);
        File.WriteAllText(csvPath,
            "Предмет;Бал\nООП;95\nБази даних;88.5\nUnity;91",
            Encoding.UTF8);

        // Колекція базового типу — абстрактного класу
        List<FileProcessor> processors = new List<FileProcessor>
        {
            new TextFileProcessor(txtPath),
            new XmlFileProcessor(xmlPath),
            new CsvFileProcessor(csvPath),
            new TextFileProcessor(Path.Combine(folder, "missing.txt")) // демонстрація обробки помилки
        };

        foreach (FileProcessor processor in processors)
        {
            processor.Process();
        }

        Directory.Delete(folder, recursive: true);
    }
}
