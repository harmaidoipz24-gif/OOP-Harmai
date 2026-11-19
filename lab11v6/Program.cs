using System;
using System.Collections.Generic;
using System.Linq;

namespace lab11v6
{
   
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Artist(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Artist other)
            {
                return Id == other.Id && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name.ToLowerInvariant());
        }

        public override string ToString()
        {
            return $"{Name} (ID: {Id})";
        }
    }

  
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int DurationSeconds { get; set; }
        public Artist Artist { get; set; }

        public Song(int id, string title, int durationSeconds, Artist artist)
        {
            Id = id;
            Title = title;
            DurationSeconds = durationSeconds;
            Artist = artist;
        }

        public string FormattedDuration => $"{DurationSeconds / 60}:{DurationSeconds % 60:D2}";

        public override string ToString()
        {
            return $"\"{Title}\" — {Artist.Name} [{FormattedDuration} / {DurationSeconds} сек.]";
        }
    }

    
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Song> Songs { get; set; }

        public Playlist(int id, string name, List<Song> songs)
        {
            Id = id;
            Name = name;
            Songs = songs ?? new List<Song>();
        }
    }

   
    public static class SongCollectionExtensions
    {
        
        public static int TotalDuration(this IEnumerable<Song> songs)
        {
            if (songs == null)
                throw new ArgumentNullException(nameof(songs), "Колекція пісень не може бути null.");

            return songs.Sum(song => song.DurationSeconds);
        }
    }

   
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

           
            Console.WriteLine("  ЛАБОРАТОРНА РОБОТА №11 | ВАРІАНТ 6: SelectMany, Пагінація, Extensions");

            var artist1 = new Artist(1, "Океан Ельзи");
            var artist2 = new Artist(2, "The Hardkiss");
            var artist3 = new Artist(3, "Бумбокс");
            var artist4 = new Artist(4, "Imagine Dragons");
            var artist5 = new Artist(5, "Linkin Park");
            var artist6 = new Artist(6, "Coldplay");

            List<Playlist> playlists = new List<Playlist>
            {
                new Playlist(1, "Український рок та хіти", new List<Song>
                {
                    new Song(1, "Обійми", 224, artist1),
                    new Song(2, "Без бою", 259, artist1),
                    new Song(3, "Журавлі", 173, artist2),
                    new Song(4, "Кораблі", 215, artist2),
                    new Song(5, "Вахтерам", 232, artist3)
                }),
                new Playlist(2, "Світова альтернатива", new List<Song>
                {
                    new Song(6, "Believer", 204, artist4),
                    new Song(7, "Radioactive", 186, artist4),
                    new Song(8, "Numb", 187, artist5),
                    new Song(9, "In the End", 216, artist5),
                    new Song(10, "Viva La Vida", 242, artist6)
                }),
                new Playlist(3, "Енергія для тренувань (Мікс)", new List<Song>
                {
                    // Повторні виконавці для демонстрації унікальності у SelectMany
                    new Song(11, "911", 209, artist1),
                    new Song(12, "Make-Up", 183, artist2),
                    new Song(13, "Thunder", 187, artist4),
                    new Song(14, "Faint", 162, artist5)
                })
            };

            Console.WriteLine($"Завантажено плейлистів: {playlists.Count}");
            foreach (var pl in playlists)
            {
                Console.WriteLine($"  • Плейлист \"{pl.Name}\" ({pl.Songs.Count} пісень)");
            }
            Console.WriteLine();

           
            Console.WriteLine(" 1. SELECTMANY: Усі унікальні виконавці з усіх плейлистів ");

            var uniqueArtists = playlists
                .SelectMany(playlist => playlist.Songs) // Розгортаємо вкладені списки Songs в один потік
                .Select(song => song.Artist)            // Отримуємо виконавця кожної пісні
                .Distinct()                             // Відкидаємо дублікати виконавців
                .ToList();

            Console.WriteLine($"Знайдено унікальних виконавців: {uniqueArtists.Count}");
            foreach (var artist in uniqueArtists)
            {
                Console.WriteLine($"  • {artist}");
            }
            Console.WriteLine();

            Console.WriteLine("2. ПАГІНАЦІЯ (Skip / Take): Усі пісні по 5 на сторінку ");

            // Отримуємо плоский список усіх пісень з усіх плейлистів
            List<Song> allSongs = playlists
                .SelectMany(playlist => playlist.Songs)
                .ToList();

            int pageSize = 5;
            int totalPages = (int)Math.Ceiling((double)allSongs.Count / pageSize);

            Console.WriteLine($"Загальна кількість пісень: {allSongs.Count} | Розмір сторінки: {pageSize} | Всього сторінок: {totalPages}\n");

            for (int currentPage = 1; currentPage <= totalPages; currentPage++)
            {
                var pageSongs = allSongs
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                Console.WriteLine($" Сторінка {currentPage} з {totalPages} ");
                int indexOnPage = (currentPage - 1) * pageSize + 1;
                foreach (var song in pageSongs)
                {
                    Console.WriteLine($"  {indexOnPage++}. {song}");
                }
                Console.WriteLine();
            }

            Console.WriteLine(" 3. CUSTOM EXTENSION: Метод розширення TotalDuration() ");

            foreach (var playlist in playlists)
            {
                // Виклик власного методу розширення на колекції playlist.Songs
                int durationSec = playlist.Songs.TotalDuration();
                TimeSpan ts = TimeSpan.FromSeconds(durationSec);

                Console.WriteLine($"  • Плейлист \"{playlist.Name}\": {durationSec} сек. ({(int)ts.TotalMinutes} хв {ts.Seconds:D2} сек.)");
            }

            // Виклик власного методу розширення для всієї бібліотеки пісень
            int totalLibraryDuration = allSongs.TotalDuration();
            TimeSpan totalTs = TimeSpan.FromSeconds(totalLibraryDuration);
            Console.WriteLine($"\n  => Загальна тривалість усіх {allSongs.Count} пісень у медіатеці: {totalLibraryDuration} сек. ({(int)totalTs.TotalMinutes} хв {totalTs.Seconds:D2} сек.)");

            Console.WriteLine("ВИСНОВОК:");
            Console.WriteLine(" 1. SelectMany зручно перетворює ієрархію (Playlist -> Songs) у плоский список.");
            Console.WriteLine(" 2. Комбінація Skip() та Take() ефективно реалізує посторінкову навігацію.");
            Console.WriteLine(" 3. Метод розширення TotalDuration() інтегрується в ланцюжки викликів IEnumerable<Song>.");
        }
    }
}