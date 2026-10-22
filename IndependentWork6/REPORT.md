# Звіт до самостійної роботи №6

**Тема:** Інтерфейс vs абстрактний клас: практичні критерії
**Виконав:** Гармай Денис

## Порівняльна таблиця, з якої виходив вибір

| Критерій | Абстрактний клас | Інтерфейс |
|---|---|---|
| Відношення | «є» (*is-a*): нащадки — різновиди одного поняття | «уміє» (*can-do*): спільна лише поведінка |
| Спільний стан (поля) | може мати | не може мати полів екземпляра |
| Спільна реалізація | так, звичайні та віртуальні методи | лише default-методи (C# 8+), без стану |
| Конструктор | так | ні |
| Кількість для одного класу | лише один базовий клас | скільки завгодно |
| Зв'язність | сильна: нащадок залежить від реалізації бази | слабка: залежність лише від контракту |

Спочатку характеристики кожного сценарію було зіставлено з цією таблицею, і лише потім написано код.

## Сценарій 1: Система сповіщень

### Обраний підхід: Інтерфейс `INotifier`

### Обґрунтування вибору

У цьому сценарії краще використовувати інтерфейс `INotifier`, оскільки:

1. **Немає спільного стану і спільних кроків.** Email потребує адреси й теми, SMS — номера телефону і розбиття на частини по 70 символів, Push — ідентифікатора пристрою. У каналів немає жодного поля чи кроку алгоритму, який можна було б винести в базовий клас. Їх об'єднує лише здатність «відправити повідомлення», а це і є визначення інтерфейсу.
2. **Множинна поведінка.** Клас може бути одночасно сповіщувачем і чимось іще. У програмі `EmailNotifier` і `SmsNotifier` реалізують ще й `IDeliveryReport`, а `PushNotifier` — ні. З абстрактним класом так не вийде: у C# можна успадкувати лише один клас.
3. **Канали можуть мати власні базові класи.** На практиці `EmailNotifier` міг би вже успадковуватися від класу SMTP-клієнта бібліотеки, а `SmsNotifier` — від HTTP-клієнта сервісу розсилки. Абстрактний базовий клас `Notifier` зайняв би це єдине місце для наслідування; інтерфейс його не займає.
4. **Слабке зв'язування (DIP).** `NotificationService` залежить лише від `INotifier` і не знає про конкретні канали. Новий канал (Telegram, Viber) додається новим класом без зміни сервісу.

**Чому абстрактний клас тут менш доречний:** абстрактний клас `Notifier` з єдиним абстрактним методом `Send` не дав би жодної спільної реалізації, але змусив би всі канали входити в одну ієрархію і забрав би можливість успадкуватися від чогось іншого. Це сильна зв'язність без жодної вигоди.

### Реалізація

```csharp
// Інтерфейс
public interface INotifier
{
    string ChannelName { get; }
    void Send(string message);
}

public interface IDeliveryReport
{
    int SentCount { get; }
}

// Реалізація 1
public class EmailNotifier : INotifier, IDeliveryReport
{
    private readonly string _address;
    public string ChannelName => "Email";
    public int SentCount { get; private set; }

    public void Send(string message)
    {
        string subject = message.Length > 20 ? message[..20] + "..." : message;
        Console.WriteLine($"[Email] Кому: {_address}");
        Console.WriteLine($"        Тема: {subject}");
        Console.WriteLine($"        Текст: {message}");
        SentCount++;
    }
}

// Реалізація 2: довге повідомлення ділиться на кілька SMS
public class SmsNotifier : INotifier, IDeliveryReport
{
    private const int MaxLength = 70;

    public void Send(string message)
    {
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

// Реалізація 3
public class PushNotifier : INotifier
{
    public void Send(string message)
    {
        string preview = message.Length > 40 ? message[..40] + "..." : message;
        Console.WriteLine($"[Push]  Пристрій {_deviceId}: {preview}");
    }
}

// Демонстрація в Main
List<INotifier> notifiers = new List<INotifier>
{
    new EmailNotifier("denys@example.com"),
    new SmsNotifier("+380 67 123 45 67"),
    new PushNotifier("android-7f3a")
};

foreach (INotifier notifier in notifiers)
{
    Console.WriteLine($"Канал: {notifier.ChannelName}");
    notifier.Send(message);
}

// Додатковий інтерфейс перевіряється через is
foreach (INotifier notifier in notifiers)
{
    if (notifier is IDeliveryReport report)
        Console.WriteLine($"{notifier.ChannelName}: надіслано {report.SentCount} повідомлень");
}
```

Цикл `foreach` викликає `Send` через посилання типу `INotifier`, а виконується реалізація конкретного каналу: Email виводить тему і текст, SMS ділить текст на частини, Push показує коротке прев'ю.

## Сценарій 2: Обробка файлів

### Обраний підхід: Абстрактний клас `FileProcessor`

### Обґрунтування вибору

У цьому сценарії краще використовувати абстрактний клас `FileProcessor`, оскільки:

1. **Спільні кроки алгоритму.** Кожен обробник виконує однакову послідовність: перевірити існування файлу, відкрити, прочитати, обробити вміст, гарантовано закрити. Абстрактний клас реалізує цю послідовність **один раз** у методі `Process()` (патерн «Шаблонний метод»), а нащадки перевизначають лише крок `ProcessContent`.
2. **Спільний стан.** Шлях до файлу, відкритий `StreamReader`, лічильник прочитаних рядків — це поля, а інтерфейс полів екземпляра мати не може.
3. **Контроль над порядком кроків.** Метод `Process()` не віртуальний, а `Open()` і `Close()` — `private`. Нащадок фізично не може «забути» закрити файл або змінити порядок кроків. Блок `try/finally` у базовому класі гарантує закриття навіть при помилці в `ProcessContent`.
4. **Чітке відношення «є».** Текстовий, XML- і CSV-обробник — це різновиди одного поняття «обробник файлу», тож ієрархія природна і стабільна.
5. **Обов'язковий конструктор.** `protected FileProcessor(string filePath)` змушує кожного нащадка передати шлях, тож об'єкт без файлу створити неможливо.

**Чому інтерфейс тут менш доречний:** інтерфейс `IFileProcessor` з методами `Open`, `ProcessContent`, `Close` змусив би **кожен** клас окремо писати відкриття, закриття і обробку помилок — це дублювання коду і ризик, що в одному з класів файл не закриється. Крім того, інтерфейс не може зберігати `StreamReader` і не може заборонити викликати кроки в неправильному порядку. Default-методи інтерфейсу (C# 8+) частково вирішують дублювання, але без полів стану реалізувати відкриття/закриття файлу в них неможливо.

### Реалізація

```csharp
public abstract class FileProcessor
{
    private readonly string _filePath;
    private StreamReader? _reader;

    public int LinesRead { get; private set; }
    public abstract string FormatName { get; }

    protected FileProcessor(string filePath)
    {
        _filePath = filePath;
    }

    // Шаблонний метод: послідовність кроків фіксована
    public void Process()
    {
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
    }

    private void Open() { /* перевірка існування і створення StreamReader */ }
    private void Close() { /* Dispose і виведення статистики */ }

    // Унікальна логіка — обов'язкова для кожного нащадка
    protected abstract void ProcessContent(string content);
}

// Реалізація 1: статистика тексту
public class TextFileProcessor : FileProcessor
{
    public override string FormatName => "TXT";
    public TextFileProcessor(string filePath) : base(filePath) { }

    protected override void ProcessContent(string content)
    {
        string[] words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"   Символів: {content.Length}, слів: {words.Length}");
    }
}

// Реалізація 2: розбір XML
public class XmlFileProcessor : FileProcessor
{
    public override string FormatName => "XML";
    public XmlFileProcessor(string filePath) : base(filePath) { }

    protected override void ProcessContent(string content)
    {
        XDocument document = XDocument.Parse(content);
        foreach (XElement student in document.Root!.Elements("student"))
            Console.WriteLine($"   - {student.Attribute("name")?.Value}");
    }
}

// Реалізація 3: підсумок CSV
public class CsvFileProcessor : FileProcessor
{
    public override string FormatName => "CSV";
    public CsvFileProcessor(string filePath) : base(filePath) { }

    protected override void ProcessContent(string content) { /* сума другого стовпця */ }
}

// Демонстрація в Main
List<FileProcessor> processors = new List<FileProcessor>
{
    new TextFileProcessor(txtPath),
    new XmlFileProcessor(xmlPath),
    new CsvFileProcessor(csvPath),
    new TextFileProcessor(Path.Combine(folder, "missing.txt"))
};

foreach (FileProcessor processor in processors)
{
    processor.Process();
}
```

Тестові файли (`notes.txt`, `students.xml`, `grades.csv`) створюються в коді через `File.WriteAllText`, тож проєкт не залежить від зовнішніх файлів. Для кожного об'єкта викликається той самий `Process()`: кроки відкриття і закриття однакові, а `ProcessContent` виконується свій. Четвертий обробник отримує неіснуючий файл і показує, що обробка помилки теж реалізована один раз у базовому класі.

## Загальні висновки: практичні критерії вибору

**Обирайте інтерфейс, якщо:**
- класи пов'язані лише спільною поведінкою, а не походженням (канали сповіщень);
- немає спільного стану і спільного коду, який варто винести;
- класу може знадобитися кілька ролей одночасно (`INotifier` + `IDeliveryReport`);
- реалізації можуть належати до різних ієрархій або сторонніх бібліотек;
- потрібне слабке зв'язування і легка заміна реалізації (наприклад, підставити тестовий `FakeNotifier`).

**Обирайте абстрактний клас, якщо:**
- нащадки є різновидами одного поняття (відношення «є»);
- є спільні поля та спільна реалізація, яку треба написати один раз;
- потрібно зафіксувати порядок кроків алгоритму і дозволити змінювати лише окремі кроки (шаблонний метод);
- потрібен конструктор, що гарантує коректну ініціалізацію;
- потрібні `protected` члени, доступні тільки нащадкам.

**Підходи можна поєднувати.** Поширений прийом — інтерфейс як публічний контракт і абстрактний клас як зручна базова реалізація: наприклад, `FileProcessor` міг би реалізовувати `IFileProcessor`. Тоді клієнтський код залежить від інтерфейсу, а нащадки отримують готові спільні кроки.

Обидва сценарії збігаються з рекомендацією методички: для непов'язаних каналів сповіщень обрано інтерфейс, для обробників файлів зі спільними кроками — абстрактний клас.

## Відповіді на контрольні запитання

1. **Які ознаки підказують, що сценарій краще моделювати через інтерфейс?** — Класи не мають спільного стану й коду; їх об'єднує лише «уміння» щось робити; класу можуть знадобитися кілька ролей; реалізації можуть мати різні базові класи; потрібна легка заміна реалізації.
2. **У чому перевага абстрактного класу для процесів зі спільними кроками?** — Спільні кроки (відкриття, закриття, обробка помилок) і стан пишуться один раз; базовий клас контролює їхній порядок через шаблонний метод, а нащадки змінюють лише свою частину й не можуть порушити алгоритм.
3. **Як порівняння двох підходів допомагає прийняти архітектурне рішення?** — Воно змушує явно перевірити критерії (спільний стан, відношення «є» чи «уміє», потреба в кількох ролях) до написання коду і показує, чим доведеться заплатити за альтернативу: дублюванням коду чи зайвою зв'язністю.
4. **Яку роль у прикладах відіграє поліморфізм?** — Колекції `List<INotifier>` і `List<FileProcessor>` містять об'єкти різних класів, але обробляються одним циклом: виклик `Send()` чи `ProcessContent()` автоматично виконує реалізацію фактичного типу. Завдяки цьому новий канал чи формат файлу додається без змін клієнтського коду.
