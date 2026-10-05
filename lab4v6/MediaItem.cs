using System;

namespace lab6v6
{
    /// <summary>
    /// Базовий клас для медіа-елементів.
    /// </summary>
    public class MediaItem
    {
        // Приватні поля (Private Fields)
        private string _title;
        private int _duration; // Тривалість у хвилинах

        // Публічні властивості (Public Properties)
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

        // Віртуальний метод (Virtual Method) — дозволяє перевизначення у похідних класах
        public virtual void Play()
        {
            Console.WriteLine($"[MediaItem] Відтворюється базовий медіа-елемент: \"{Title}\" (Тривалість: {Duration} хв.)");
        }

        // Метод базового класу для демонстрації приховування за допомогою модифікатора new
        public string GetMediaType()
        {
            return "Базовий тип: Загальний медіа-елемент (MediaItem)";
        }
    }
}