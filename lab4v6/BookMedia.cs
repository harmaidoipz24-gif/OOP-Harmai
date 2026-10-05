using System;

namespace lab6v6
{
    /// <summary>
    /// Похідний клас для аудіокниг / цифрових книг.
    /// </summary>
    public class BookMedia : MediaItem
    {
        // Приватне поле та публічна властивість
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

        // Конструктор, що викликає конструктор базового класу за допомогою base(...)
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

        // Приховування (new) члена базового класу
        public new string GetMediaType()
        {
            return "Похідний тип: Друкована / Аудіокнига (BookMedia)";
        }
    }
}