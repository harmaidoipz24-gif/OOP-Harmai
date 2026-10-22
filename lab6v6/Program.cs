using System;
using System.Collections.Generic;

namespace lab6v6
{

    public class MediaItem
    {
        // Приватні поля
        private string _title;
        private int _duration; // Тривалість у хвилинах

        // Публічні властивості з перевіркою коректності даних
        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва медіафайлу не може бути порожньою.");
                _title = value;
            }
        }

        public int Duration
        {
            get => _duration;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Тривалість не може бути від'ємною.");
                _duration = value;
            }
        }

        // Конструктор базового класу
        public MediaItem(string title, int duration)
        {
            Title = title;
            Duration = duration;
        }

        // Віртуальний метод для перевизначення у похідних класах
        public virtual void Play()
        {
            Console.WriteLine($"[MediaItem] Відтворюється базовий медіа-елемент: \"{Title}\" (Тривалість: {Duration} хв.)");
        }

        // Метод базового класу для демонстрації приховування за допомогою new
        public string GetMediaType()
        {
            return "Базовий тип: Загальний медіа-елемент (MediaItem)";
        }
    }

    public class BookMedia : MediaItem
    {
        private string _author;

        public string Author
        {
            get => _author;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ім'я автора не може бути порожнім.");
                _author = value;
            }
        }

        // Конструктор, що викликає базовий за допомогою base(...)
        public BookMedia(string title, int duration, string author) 
            : base(title, duration)
        {
            Author = author;
        }

        // Перевизначення (override) віртуального методу базового класу
        public override void Play()
        {
            Console.WriteLine($"[BookMedia - OVERRIDE] Озвучується аудіокнига: \"{Title}\" | Автор: {Author} (Тривалість: {Duration} хв.)");
        }

        // Унікальний метод класу BookMedia
        public void ReadSample()
        {
            Console.WriteLine($"[BookMedia - Унікальний метод] Відкрито безкоштовний уривок книги \"{Title}\" (Автор: {Author}).");
        }

        // Явне приховування (new) методу базового класу
        public new string GetMediaType()
        {
            return "Похідний тип: Друкована / Аудіокнига (BookMedia)";
        }
    }

    public class MovieMedia : MediaItem
    {
        private string _director;

        public string Director
        {
            get => _director;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ім'я режисера не може бути порожнім.");
                _director = value;
            }
        }

        // Конструктор, що викликає базовий за допомогою base(...)
        public MovieMedia(string title, int duration, string director) 
            : base(title, duration)
        {
            Director = director;
        }

        // Перевизначення (override) віртуального методу базового класу
        public override void Play()
        {
            Console.WriteLine($"[MovieMedia - OVERRIDE] Відтворюється фільм: \"{Title}\" | Режисер: {Director} (Тривалість: {Duration} хв.)");
        }

        // Унікальний метод класу MovieMedia
        public void ShowTrailer()
        {
            Console.WriteLine($"[MovieMedia - Унікальний метод] Запущено офіційний трейлер до фільму \"{Title}\" (Режисер: {Director}).");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // Налаштування кодування для коректного виводу кирилиці в консолі
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №6 | ВАРІАНТ 6: MediaItem -> BookMedia / MovieMedia");

            // 1. Створення об'єктів базового та похідних класів
            MediaItem genericItem = new MediaItem("Лекція з ООП", 80);
            BookMedia book = new BookMedia("Кобзар", 320, "Тарас Шевченко");
            MovieMedia movie = new MovieMedia("Інтерстеллар", 169, "Крістофер Нолан");

            // 2. Демонстрація виклику унікальних методів
            Console.WriteLine("1. ВИКЛИК УНІКАЛЬНИХ МЕТОДІВ ПОХІДНИХ КЛАСІВ");
            book.ReadSample();
            movie.ShowTrailer();
            Console.WriteLine();

            // 3. Демонстрація ПОЛІМОРФІЗМУ (virtual / override)
            Console.WriteLine(" 2. ДЕМОНСТРАЦІЯ ПОЛІМОРФІЗМУ (virtual / override)");
            List<MediaItem> playlist = new List<MediaItem>
            {
                genericItem,
                book,
                movie
            };

            Console.WriteLine("Перебір елементів у списку List<MediaItem> через посилання базового типу:");
            foreach (MediaItem item in playlist)
            {
                // Динамічний виклик перевизначеного методу залежно від реального типу об'єкта
                item.Play();
            }
            Console.WriteLine();

            // 4. Демонстрація різниці між OVERRIDE та NEW
            Console.WriteLine("3. ДЕМОНСТРАЦІЯ РІЗНИЦІ МІЖ OVERRIDE ТА NEW");

            // Створюємо об'єкт BookMedia та зберігаємо посилання у двох змінних різних типів:
            BookMedia bookRef = new BookMedia("1984", 280, "Джордж Орвелл");
            MediaItem baseRef = bookRef; // Приведення до базового типу MediaItem

            Console.WriteLine("Один і той самий об'єкт '1984', викликаний через різні типи посилань:\n");

            Console.WriteLine("[А] Виклик через посилання типу BookMedia (похідний клас):");
            Console.WriteLine($"    * GetMediaType() [new]: {bookRef.GetMediaType()}");
            Console.Write("    * ");
            bookRef.Play();

            Console.WriteLine("\n[Б] Виклик через посилання типу MediaItem (базовий клас):");
            Console.WriteLine($"    * GetMediaType() [new]: {baseRef.GetMediaType()}");
            Console.Write("    * ");
            baseRef.Play();

            Console.WriteLine("ВИСНОВОК ЩОДО ДЕМОНСТРАЦІЇ:");
            Console.WriteLine(" 1. OVERRIDE (Play): Незалежно від типу посилання викликається перевизначений");
            Console.WriteLine("    метод реального об'єкта в пам'яті (BookMedia).");
            Console.WriteLine(" 2. NEW (GetMediaType): Результат залежить від ТИПУ ПОСИЛАННЯ.");
            Console.WriteLine("    Через MediaItem викликається метод базового класу, а через BookMedia — новий.");
        }
    }
}