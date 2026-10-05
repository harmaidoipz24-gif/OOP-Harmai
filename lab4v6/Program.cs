using System;
using System.Collections.Generic;

namespace lab6v6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Встановлюємо кодування консолі для коректного відображення кирилиці
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("ЛАБОРАТОРНА РОБОТА №4 | ВАРІАНТ 6: MediaItem -> BookMedia / MovieMedia");
           

            // 1. Створення об'єктів базового та похідних класів
            MediaItem genericItem = new MediaItem("Лекція з ООП", 80);
            BookMedia book = new BookMedia("Кобзар", 320, "Тарас Шевченко");
            MovieMedia movie = new MovieMedia("Інтерстеллар", 169, "Крістофер Нолан");

            // 2. Виклики унікальних методів похідних класів
            Console.WriteLine("1. ВИКЛИК УНІКАЛЬНИХ МЕТОДІВ ПОХІДНИХ КЛАСІВ");
            book.ReadSample();
            movie.ShowTrailer();
            Console.WriteLine();

            // 3. Демонстрація ПОЛІМОРФІЗМУ (virtual / override)
            Console.WriteLine("2. ДЕМОНСТРАЦІЯ ПОЛІМОРФІЗМУ (virtual / override)");
            List<MediaItem> playlist = new List<MediaItem>
            {
                genericItem,
                book,
                movie
            };

            Console.WriteLine("Перебір елементів у масиві/списку типу List<MediaItem>:");
            foreach (MediaItem item in playlist)
            {
                // Поліморфний виклик: запуск відповідної реалізації залежно від об'єкта
                item.Play();
            }
            Console.WriteLine();

            // 4. Демонстрація різниці між OVERRIDE та NEW
            Console.WriteLine(" 3. ДЕМОНСТРАЦІЯ РІЗНИЦІ МІЖ OVERRIDE ТА NEW ");

            // Створюємо об'єкт BookMedia та зберігаємо посилання в двох змінних різних типів:
            BookMedia bookRef = new BookMedia("1984", 280, "Джордж Орвелл");
            MediaItem baseRef = bookRef; // Неявне приведення до базового типу

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
            Console.WriteLine(" 1. Для OVERRIDE (Play): Викликається перевизначена версія об'єкта НА ГУРТІ (BookMedia),");
            Console.WriteLine("    незалежно від того, яким є тип посилання (MediaItem чи BookMedia).");
            Console.WriteLine(" 2. Для NEW (GetMediaType): Приховування залежить від ТИПУ ПОСИЛАННЯ.");
            Console.WriteLine("    Через MediaItem викликається метод базового класу, а через BookMedia — новий.");
        
        }
    }
}