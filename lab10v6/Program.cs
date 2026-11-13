using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace lab16v6
{
    
    public class Director
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }

        public Director(int id, string name, string country)
        {
            Id = id;
            Name = name;
            Country = country;
        }
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int DirectorId { get; set; }
        public int ReleaseYear { get; set; }

        public Movie(int id, string title, int directorId, int releaseYear)
        {
            Id = id;
            Title = title;
            DirectorId = directorId;
            ReleaseYear = releaseYear;
        }
    }

   
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("  ЛАБОРАТОРНА РОБОТА №10 | ВАРІАНТ 6: LINQ (Фільтри, Join, Агрегації)");

           
            List<Director> directors = new List<Director>
            {
                new Director(1, "Christopher Nolan", "United Kingdom"),
                new Director(2, "Quentin Tarantino", "USA"),
                new Director(3, "Denis Villeneuve", "Canada"),
                new Director(4, "Martin Scorsese", "USA"),
                new Director(5, "Steven Spielberg", "USA"),
                new Director(6, "Ridley Scott", "United Kingdom"),
                new Director(7, "James Cameron", "Canada"),
                new Director(8, "David Fincher", "USA"),
                new Director(9, "Hayao Miyazaki", "Japan"),
                new Director(10, "Greta Gerwig", "USA")
            };

            List<Movie> movies = new List<Movie>
            {
                // Фільми Christopher Nolan (включаючи 1998 та 2000 р. для перевірки умови > 2000)
                new Movie(1, "Following", 1, 1998),
                new Movie(2, "Memento", 1, 2000),
                new Movie(3, "Batman Begins", 1, 2005),
                new Movie(4, "The Dark Knight", 1, 2008),
                new Movie(5, "Inception", 1, 2010),
                new Movie(6, "Interstellar", 1, 2014),
                new Movie(7, "Oppenheimer", 1, 2023),

                // Фільми інших режисерів
                new Movie(8, "Pulp Fiction", 2, 1994),
                new Movie(9, "Inglourious Basterds", 2, 2009),
                new Movie(10, "Dune: Part Two", 3, 2024),
                new Movie(11, "Arrival", 3, 2016),
                new Movie(12, "The Wolf of Wall Street", 4, 2013),
                new Movie(13, "Schindler's List", 5, 1993),
                new Movie(14, "Gladiator", 6, 2000),
                new Movie(15, "Avatar: The Way of Water", 7, 2022),
                new Movie(16, "Fight Club", 8, 1999),
                new Movie(17, "Spirited Away", 9, 2001),
                new Movie(18, "Barbie", 10, 2023)
            };

            Console.WriteLine($"Завантажено тестових даних: {directors.Count} режисерів та {movies.Count} фільмів.\n");

            Stopwatch sw = new Stopwatch();
            var performanceStats = new List<(string QueryName, long QuerySyntaxTicks, long MethodSyntaxTicks)>();

          
            Console.WriteLine(" ЗАПИТ 1: Фільми режисера 'Christopher Nolan' після 2000 року ");

            // 1A. Синтаксис запитів (Query Syntax)
            sw.Restart();
            var q1Query = (from m in movies
                           join d in directors on m.DirectorId equals d.Id
                           where d.Name == "Christopher Nolan" && m.ReleaseYear > 2000
                           select m).ToList();
            sw.Stop();
            long q1QueryTicks = sw.ElapsedTicks;
            double q1QueryMs = sw.Elapsed.TotalMilliseconds;

            // 1B. Синтаксис методів (Method Syntax)
            sw.Restart();
            var q1Method = movies
                .Join(directors, m => m.DirectorId, d => d.Id, (m, d) => new { Movie = m, Director = d })
                .Where(x => x.Director.Name == "Christopher Nolan" && x.Movie.ReleaseYear > 2000)
                .Select(x => x.Movie)
                .ToList();
            sw.Stop();
            long q1MethodTicks = sw.ElapsedTicks;
            double q1MethodMs = sw.Elapsed.TotalMilliseconds;

            performanceStats.Add(("Запит 1 (Where + Join)", q1QueryTicks, q1MethodTicks));

            Console.WriteLine($"[Query Syntax]  Час: {q1QueryMs:F4} мс ({q1QueryTicks} тіків)");
            Console.WriteLine($"[Method Syntax] Час: {q1MethodMs:F4} мс ({q1MethodTicks} тіків)");
            Console.WriteLine("Результат:");
            foreach (var m in q1Method)
            {
                Console.WriteLine($"  • \"{m.Title}\" ({m.ReleaseYear} р.)");
            }
            Console.WriteLine();

           
            Console.WriteLine("ЗАПИТ 2: Об'єднання фільмів з їхніми режисерами (Join)");

            // 2A. Синтаксис запитів (Query Syntax)
            sw.Restart();
            var q2Query = (from m in movies
                           join d in directors on m.DirectorId equals d.Id
                           select new
                           {
                               MovieTitle = m.Title,
                               DirectorName = d.Name
                           }).ToList();
            sw.Stop();
            long q2QueryTicks = sw.ElapsedTicks;
            double q2QueryMs = sw.Elapsed.TotalMilliseconds;

            // 2B. Синтаксис методів (Method Syntax)
            sw.Restart();
            var q2Method = movies.Join(
                directors,
                m => m.DirectorId,
                d => d.Id,
                (m, d) => new
                {
                    MovieTitle = m.Title,
                    DirectorName = d.Name
                }).ToList();
            sw.Stop();
            long q2MethodTicks = sw.ElapsedTicks;
            double q2MethodMs = sw.Elapsed.TotalMilliseconds;

            performanceStats.Add(("Запит 2 (Inner Join)", q2QueryTicks, q2MethodTicks));

            Console.WriteLine($"[Query Syntax]  Час: {q2QueryMs:F4} мс ({q2QueryTicks} тіків)");
            Console.WriteLine($"[Method Syntax] Час: {q2MethodMs:F4} мс ({q2MethodTicks} тіків)");
            Console.WriteLine($"Результат (показано перші 8 із {q2Method.Count} записів):");
            foreach (var item in q2Method.Take(8))
            {
                Console.WriteLine($"  • \"{item.MovieTitle}\" — Режисер: {item.DirectorName}");
            }
            Console.WriteLine();

         
            Console.WriteLine("ЗАПИТ 3: Кількість фільмів для кожного режисера (GroupJoin + Count)");

            // 3A. Синтаксис запитів (Query Syntax)
            sw.Restart();
            var q3Query = (from d in directors
                           join m in movies on d.Id equals m.DirectorId into directorMovies
                           select new
                           {
                               DirectorName = d.Name,
                               MoviesCount = directorMovies.Count()
                           }).ToList();
            sw.Stop();
            long q3QueryTicks = sw.ElapsedTicks;
            double q3QueryMs = sw.Elapsed.TotalMilliseconds;

            // 3B. Синтаксис методів (Method Syntax)
            sw.Restart();
            var q3Method = directors.GroupJoin(
                movies,
                d => d.Id,
                m => m.DirectorId,
                (d, directorMovies) => new
                {
                    DirectorName = d.Name,
                    MoviesCount = directorMovies.Count()
                }).ToList();
            sw.Stop();
            long q3MethodTicks = sw.ElapsedTicks;
            double q3MethodMs = sw.Elapsed.TotalMilliseconds;

            performanceStats.Add(("Запит 3 (GroupJoin + Count)", q3QueryTicks, q3MethodTicks));

            Console.WriteLine($"[Query Syntax]  Час: {q3QueryMs:F4} мс ({q3QueryTicks} тіків)");
            Console.WriteLine($"[Method Syntax] Час: {q3MethodMs:F4} мс ({q3MethodTicks} тіків)");
            Console.WriteLine("Результат:");
            foreach (var stat in q3Method)
            {
                Console.WriteLine($"  • {stat.DirectorName,-20} : {stat.MoviesCount} фільм(ів)");
            }
            Console.WriteLine();

           
            Console.WriteLine(" ЗАПИТ 4: Найновіший фільм у фільмотеці ");

            // 4A. Синтаксис запитів (Query Syntax + FirstOrDefault)
            sw.Restart();
            var q4Query = (from m in movies
                           orderby m.ReleaseYear descending
                           select m).FirstOrDefault();
            sw.Stop();
            long q4QueryTicks = sw.ElapsedTicks;
            double q4QueryMs = sw.Elapsed.TotalMilliseconds;

            // 4B. Синтаксис методів (Method Syntax - однопрохідний MaxBy)
            sw.Restart();
            var q4Method = movies.MaxBy(m => m.ReleaseYear);
            sw.Stop();
            long q4MethodTicks = sw.ElapsedTicks;
            double q4MethodMs = sw.Elapsed.TotalMilliseconds;

            performanceStats.Add(("Запит 4 (OrderBy vs MaxBy)", q4QueryTicks, q4MethodTicks));

            Console.WriteLine($"[Query Syntax (OrderBy)] Час: {q4QueryMs:F4} мс ({q4QueryTicks} тіків)");
            Console.WriteLine($"[Method Syntax (MaxBy)]  Час: {q4MethodMs:F4} мс ({q4MethodTicks} тіків)");
            if (q4Method != null)
            {
                Console.WriteLine($"Результат: \"{q4Method.Title}\" ({q4Method.ReleaseYear} рік випуску)");
            }
            Console.WriteLine();

            
            Console.WriteLine("  АНАЛІЗ ПРОДУКТИВНОСТІ LINQ-ЗАПИТІВ (Stopwatch)");
            Console.WriteLine($"{"Запит",-30} | {"Query Syntax (тіки)",-20} | {"Method Syntax (тіки)",-20}");

            foreach (var item in performanceStats)
            {
                Console.WriteLine($"{item.QueryName,-30} | {item.QuerySyntaxTicks,-20} | {item.MethodSyntaxTicks,-20}");
            }

            Console.WriteLine("Висновки:");
            Console.WriteLine(" 1. Синтаксис запитів та синтаксис методів компілюються в однаковий IL-код,");
            Console.WriteLine("    тому після прогріву JIT їхня швидкість є практично ідентичною.");
            Console.WriteLine(" 2. Найбільший час у Запиті 1 (Query Syntax) зумовлений холодним стартом");
            Console.WriteLine("    (JIT-компіляцією LINQ-методів при першому виклику).");
            Console.WriteLine(" 3. У Запиті 4 метод MaxBy() працює за O(N) без повного сортування колекції,");
            Console.WriteLine("    що швидше за OrderByDescending().First() зі складністю O(N log N).");
            
    }
}
}