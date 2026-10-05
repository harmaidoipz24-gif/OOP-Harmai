using System;

namespace lab6v6
{
    /// Похідний клас для фільмів.
    public class MovieMedia : MediaItem
    {
        // Приватне поле та публічна властивість
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

        // Конструктор, що викликає конструктор базового класу за допомогою base(...)
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
}