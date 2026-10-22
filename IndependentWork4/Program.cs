using System.Text;

namespace IndependentWork4;

public class LibraryItem
{
    private readonly string _uniqueId;          // ЗМІНА (п.4): нове приватне поле
    private string _title;
    private int _publicationYear;

    public string UniqueId => _uniqueId;        // ЗМІНА (п.4): нова read-only властивість

    public string Title
    {
        get => _title;
        set => _title = value;
    }

    public int PublicationYear
    {
        get => _publicationYear;
        set => _publicationYear = value;
    }

    // ЗМІНА (п.4): було LibraryItem(string title, int publicationYear)
    public LibraryItem(string uniqueId, string title, int publicationYear)
    {
        _uniqueId = uniqueId;
        _title = title;
        _publicationYear = publicationYear;
    }

    public virtual void DisplayDetails()
    {
        Console.WriteLine($"[{UniqueId}] \"{Title}\", рік видання: {PublicationYear}");
    }
}

public class Book : LibraryItem
{
    private string _author;

    public string Author
    {
        get => _author;
        set => _author = value;
    }

    // ЗМІНА (п.4): було Book(string title, int publicationYear, string author) : base(title, publicationYear)
    public Book(string uniqueId, string title, int publicationYear, string author)
        : base(uniqueId, title, publicationYear)
    {
        _author = author;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails();
        Console.WriteLine($"   Автор: {Author}");
    }
}

public class Journal : LibraryItem
{
    private int _issueNumber;

    public int IssueNumber
    {
        get => _issueNumber;
        set => _issueNumber = value;
    }

    // ЗМІНА (п.4): було Journal(string title, int publicationYear, int issueNumber) : base(title, publicationYear)
    public Journal(string uniqueId, string title, int publicationYear, int issueNumber)
        : base(uniqueId, title, publicationYear)
    {
        _issueNumber = issueNumber;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails();
        Console.WriteLine($"   Номер випуску: {IssueNumber}");
    }
}

// Пункт 5: новий клас успадковується від Book, а не від LibraryItem напряму.
public class AudioBook : Book
{
    private string _narrator;

    public string Narrator
    {
        get => _narrator;
        set => _narrator = value;
    }

    // У методичці сигнатура без uniqueId, але після п.4 його обов'язково треба
    // передати по ланцюжку AudioBook -> Book -> LibraryItem.
    public AudioBook(string uniqueId, string title, int publicationYear, string author, string narrator)
        : base(uniqueId, title, publicationYear, author)
    {
        _narrator = narrator;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails(); // виведе базову інформацію і автора
        Console.WriteLine($"   Оповідач: {Narrator}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Пункт 3: початкова ієрархія — Book і Journal
        Console.WriteLine("Book і Journal");
        Book book = new Book("BK-001", "Кобзар", 1840, "Тарас Шевченко");
        Journal journal = new Journal("JR-042", "Наука і життя", 2026, 9);
        book.DisplayDetails();
        journal.DisplayDetails();

        // Пункт 5: AudioBook
        Console.WriteLine();
        Console.WriteLine("AudioBook");
        AudioBook audioBook = new AudioBook("AB-007", "Тигролови", 1944, "Іван Багряний", "Остап Ступка");
        audioBook.DisplayDetails();

        // Поліморфний виклик через посилання на базовий тип
        Console.WriteLine();
        Console.WriteLine("Уся ієрархія через List<LibraryItem>");
        List<LibraryItem> catalog = new List<LibraryItem> { book, journal, audioBook };
        foreach (LibraryItem item in catalog)
        {
            Console.WriteLine($"Тип: {item.GetType().Name}");
            item.DisplayDetails();
        }
    }
}
