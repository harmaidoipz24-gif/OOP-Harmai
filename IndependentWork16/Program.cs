using System.Globalization;
using System.Text;

namespace IndependentWork16;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        List<Document> documents = CreateTestDocuments();

        InvertedIndex index = new InvertedIndex();
        foreach (Document doc in documents)
        {
            index.AddDocument(doc);
        }

        Console.WriteLine($"Loaded {index.DocumentCount} documents");
        Console.WriteLine($"Index contains {index.UniqueWords} unique words");
        Console.WriteLine();

        SearchDemo(index);
        RunPerformanceTests(documents);

        // Інтерактивний режим можна пропустити: dotnet run -- --no-interactive
        if (!args.Contains("--no-interactive"))
        {
            InteractiveSearch(index);
        }
    }

    private static List<Document> CreateTestDocuments()
    {
        return new List<Document>
        {
            new Document
            {
                Id = 1,
                Title = "Introduction to C#",
                Content = "C# is a modern object-oriented programming language developed by Microsoft."
            },
            new Document
            {
                Id = 2,
                Title = "Java Basics",
                Content = "Java is a popular programming language known for its portability and performance."
            },
            new Document
            {
                Id = 3,
                Title = "Python Overview",
                Content = "Python is a high-level programming language with simple syntax."
            },
            new Document
            {
                Id = 4,
                Title = "C# Advanced Features",
                Content = "C# supports LINQ, async/await, and many modern programming paradigms."
            },
            new Document
            {
                Id = 5,
                Title = "Comparing Languages",
                Content = "C#, Java, and Python are among the most popular programming languages today."
            }
        };
    }

    private static void SearchDemo(InvertedIndex index)
    {
        Console.WriteLine("Демонстрація пошуку");

        PrintResults("SearchSingleWord(\"python\")", index.SearchSingleWord("python"));
        PrintResults("SearchSingleWord(\"C#\")", index.SearchSingleWord("C#"));
        PrintResults("SearchMultipleWords(\"programming\", \"language\") — AND", index.SearchMultipleWords("programming", "language"));
        PrintResults("SearchMultipleWords(\"c#\", \"modern\") — AND", index.SearchMultipleWords("c#", "modern"));
        PrintResults("SearchAnyWord(\"java\", \"python\") — OR", index.SearchAnyWord("java", "python"));
        PrintResults("SearchSingleWord(\"rust\") — немає в індексі", index.SearchSingleWord("rust"));

        Console.WriteLine("SearchRanked(\"c#\", \"java\", \"popular\") — OR з ранжуванням:");
        foreach ((Document doc, int matches) in index.SearchRanked("c#", "java", "popular"))
        {
            Console.WriteLine($"   {doc} — збігів: {matches}");
        }
        Console.WriteLine();
    }

    private static void PrintResults(string title, List<Document> results)
    {
        string found = results.Count == 0 ? "нічого не знайдено" : string.Join(", ", results.Select(d => d.Id));
        Console.WriteLine($"{title}: {found}");
    }

    private static void RunPerformanceTests(List<Document> baseDocuments)
    {
        Console.WriteLine("Вимірювання продуктивності (слово для пошуку: \"python\")");
        Console.WriteLine("Час — середній на один пошук; побудова індексу — один раз для всієї колекції.");
        Console.WriteLine();

        List<PerformanceResult> results = new List<PerformanceResult>();
        foreach (int count in new[] { 10, 100, 1000, 10000 })
        {
            List<Document> documents = GenerateDocuments(baseDocuments, count);
            PerformanceResult result = PerformanceTest.CompareSearchMethods(documents, "python");
            results.Add(result);

            Console.WriteLine($"{count} документів:");
            Console.WriteLine($"   Linear search:  {result.LinearTicks} ticks ({result.LinearMicroseconds:F2} мкс)");
            Console.WriteLine($"   Inverted Index: {result.IndexTicks} ticks ({result.IndexMicroseconds:F3} мкс)");
            Console.WriteLine($"   Побудова індексу: {result.BuildMilliseconds:F2} мс");
            Console.WriteLine($"   Linear found: {result.LinearFound}, Index found: {result.IndexFound} " +
                              (result.LinearFound == result.IndexFound ? "(збігається)" : "(ПОМИЛКА: результати різні!)"));
        }

        // Готова таблиця для REPORT.md
        Console.WriteLine();
        Console.WriteLine("Таблиця для REPORT.md:");
        Console.WriteLine("| Документів | Лінійний пошук, мкс | Індекс, мкс | Прискорення | Побудова індексу, мс | Окупність, запитів | Знайдено (лін./індекс) |");
        Console.WriteLine("|---|---|---|---|---|---|---|");
        foreach (PerformanceResult r in results)
        {
            Console.WriteLine($"| {r.DocumentCount} | {r.LinearMicroseconds:F2} | {r.IndexMicroseconds:F3} | ×{r.Speedup:F0} | {r.BuildMilliseconds:F2} | {Math.Ceiling(r.BreakEvenQueries)} | {r.LinearFound} / {r.IndexFound} |");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Генерує потрібну кількість документів: базові тексти + випадкові слова зі словника.
    /// Random з фіксованим seed робить результати відтворюваними.
    /// </summary>
    private static List<Document> GenerateDocuments(List<Document> baseDocuments, int count)
    {
        string[] vocabulary =
        {
            "data", "algorithm", "compiler", "runtime", "memory", "thread", "network", "database",
            "server", "client", "cloud", "testing", "debugging", "framework", "library", "interface",
            "class", "object", "method", "property", "generic", "collection", "query", "index",
            "performance", "security", "design", "pattern", "module", "package"
        };

        Random random = new Random(42);
        List<Document> documents = new List<Document>(count);

        for (int i = 0; i < count; i++)
        {
            Document source = baseDocuments[i % baseDocuments.Count];
            int extraWords = random.Next(5, 15);
            StringBuilder content = new StringBuilder(source.Content);

            for (int j = 0; j < extraWords; j++)
            {
                content.Append(' ').Append(vocabulary[random.Next(vocabulary.Length)]);
            }

            documents.Add(new Document
            {
                Id = i + 1,
                Title = $"{source.Title} #{i + 1}",
                Content = content.ToString()
            });
        }

        return documents;
    }

    private static void InteractiveSearch(InvertedIndex index)
    {
        while (true)
        {
            Console.Write("Enter search query (or 'exit'): ");
            string? query = Console.ReadLine();

            if (query == null || query.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            string[] words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                continue;

            Console.WriteLine();
            Console.WriteLine("Search Results (AND)");
            List<Document> results = index.SearchMultipleWords(words);
            foreach (Document doc in results)
            {
                Console.WriteLine(doc);
                string preview = doc.Content.Length > 100 ? doc.Content.Substring(0, 100) + "..." : doc.Content;
                Console.WriteLine($"  {preview}");
            }

            Console.WriteLine();
            Console.WriteLine($"Found {results.Count} documents");
            Console.WriteLine();
        }
    }
}
