using System.Text;
using System.Text.RegularExpressions;

namespace IndependentWork14;

// Самостійна робота №14. Вивчення Dictionary<TKey, TValue> та HashSet<T> у бібліотеках .NET.
// Виконав: Гармай Денис
// Звіт з аналізом open-source прикладів і таблицею складності — REPORT.md, приклад виводу — README.md.

// Сценарій 1: Кеш метаданих файлів (Dictionary)

public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string Hash { get; set; } = string.Empty;

    public override string ToString() =>
        $"{FileName}, {Size} байт, змінено {LastModified:dd.MM.yyyy HH:mm}, hash {Hash}";
}

public class FileMetadataCache
{
    // Шляхи у Windows нечутливі до регістру, тому використовуємо custom comparer
    private readonly Dictionary<string, FileMetadata> _cache =
        new Dictionary<string, FileMetadata>(StringComparer.OrdinalIgnoreCase);

    public int Count => _cache.Count;

    // Додає новий запис або оновлює наявний (O(1))
    public void Add(FileMetadata metadata)
    {
        _cache[metadata.FileName] = metadata;
    }

    // Повертає null, якщо файл не закешовано (O(1))
    public FileMetadata? Get(string fileName)
    {
        return _cache.TryGetValue(fileName, out FileMetadata? metadata) ? metadata : null;
    }

    public bool Remove(string fileName) => _cache.Remove(fileName);

    public bool Contains(string fileName) => _cache.ContainsKey(fileName);
}

// Сценарій 2: Унікальні теги статей (HashSet)

public class TagManager
{
    // ArticleId -> множина тегів; теги нечутливі до регістру ("CSharp" == "csharp")
    private readonly Dictionary<string, HashSet<string>> _articleTags = new Dictionary<string, HashSet<string>>();

    public void AddTag(string articleId, string tag)
    {
        if (!_articleTags.TryGetValue(articleId, out HashSet<string>? tags))
        {
            tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _articleTags[articleId] = tags;
        }

        // HashSet.Add сам відкидає дублікати і повертає false
        if (!tags.Add(tag))
            Console.WriteLine($"   Тег \"{tag}\" уже є у статті {articleId} — дублікат не додано");
    }

    public void RemoveTag(string articleId, string tag)
    {
        if (_articleTags.TryGetValue(articleId, out HashSet<string>? tags))
            tags.Remove(tag);
    }

    public HashSet<string> GetTags(string articleId)
    {
        return _articleTags.TryGetValue(articleId, out HashSet<string>? tags)
            ? new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    // Об'єднання тегів усіх статей (UnionWith)
    public HashSet<string> GetAllUniqueTags()
    {
        HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (HashSet<string> tags in _articleTags.Values)
            result.UnionWith(tags);
        return result;
    }

    // Перетин тегів двох статей (IntersectWith)
    public HashSet<string> FindCommonTags(string articleId1, string articleId2)
    {
        HashSet<string> result = GetTags(articleId1);
        result.IntersectWith(GetTags(articleId2));
        return result;
    }

    // Теги, які є в першій статті, але відсутні в другій (ExceptWith)
    public HashSet<string> FindUniqueTags(string articleId, string otherArticleId)
    {
        HashSet<string> result = GetTags(articleId);
        result.ExceptWith(GetTags(otherArticleId));
        return result;
    }
}

// Сценарій 3 (додатково): Простий пошуковий індекс (Dictionary + HashSet)

public class SearchIndex
{
    // Слово -> множина документів, де воно зустрічається
    private readonly Dictionary<string, HashSet<int>> _invertedIndex = new Dictionary<string, HashSet<int>>();

    public int WordCount => _invertedIndex.Count;

    public void AddDocument(int docId, string content)
    {
        foreach (Match match in Regex.Matches(content.ToLowerInvariant(), @"[\w#]+"))
        {
            string word = match.Value;
            if (!_invertedIndex.TryGetValue(word, out HashSet<int>? docs))
            {
                docs = new HashSet<int>();
                _invertedIndex[word] = docs;
            }
            docs.Add(docId);
        }
    }

    public HashSet<int> Search(string word)
    {
        return _invertedIndex.TryGetValue(word.ToLowerInvariant(), out HashSet<int>? docs)
            ? new HashSet<int>(docs) // копія, щоб зовнішній код не зіпсував індекс
            : new HashSet<int>();
    }

    // Документи, що містять УСІ слова (перетин множин)
    public HashSet<int> SearchMultiple(params string[] words)
    {
        if (words.Length == 0)
            return new HashSet<int>();

        HashSet<int> result = Search(words[0]);
        for (int i = 1; i < words.Length; i++)
            result.IntersectWith(Search(words[i]));
        return result;
    }
}

// Демонстрація custom comparer для об'єктів

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class PersonNameComparer : IEqualityComparer<Person>
{
    public bool Equals(Person? x, Person? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Name == y.Name;
    }

    // Хеш має рахуватися з тих самих полів, що й Equals
    public int GetHashCode(Person obj) => obj.Name.GetHashCode();
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        FileMetadataCacheDemo();
        TagManagerDemo();
        SearchIndexDemo();
        ComparerDemo();
    }

    private static void FileMetadataCacheDemo()
    {
        Console.WriteLine("Сценарій 1. FileMetadataCache (Dictionary)");

        FileMetadataCache cache = new FileMetadataCache();
        cache.Add(new FileMetadata { FileName = "report.docx", Size = 48_213, LastModified = new DateTime(2026, 11, 2, 14, 30, 0), Hash = "a1f3c9" });
        cache.Add(new FileMetadata { FileName = "photo.png", Size = 1_204_551, LastModified = new DateTime(2026, 10, 28, 9, 15, 0), Hash = "77be02" });
        cache.Add(new FileMetadata { FileName = "Program.cs", Size = 6_120, LastModified = new DateTime(2026, 11, 4, 18, 5, 0), Hash = "c0de42" });

        Console.WriteLine($"У кеші файлів: {cache.Count}");
        Console.WriteLine($"Get(\"REPORT.DOCX\"): {cache.Get("REPORT.DOCX")}");
        Console.WriteLine($"Contains(\"photo.png\"): {cache.Contains("photo.png")}");
        Console.WriteLine($"Remove(\"photo.png\"): {cache.Remove("photo.png")}");
        Console.WriteLine($"Contains(\"photo.png\") після видалення: {cache.Contains("photo.png")}");
        Console.WriteLine($"Get(\"missing.txt\") повертає null: {cache.Get("missing.txt") is null}");
        Console.WriteLine();
    }

    private static void TagManagerDemo()
    {
        Console.WriteLine("Сценарій 2. TagManager (HashSet)");

        TagManager manager = new TagManager();
        manager.AddTag("post-1", "csharp");
        manager.AddTag("post-1", "dotnet");
        manager.AddTag("post-1", "collections");
        manager.AddTag("post-1", "CSharp"); // дублікат з іншим регістром

        manager.AddTag("post-2", "csharp");
        manager.AddTag("post-2", "linq");
        manager.AddTag("post-2", "collections");

        manager.AddTag("post-3", "python");
        manager.AddTag("post-3", "beginners");

        Console.WriteLine($"Усі унікальні теги (UnionWith): {Format(manager.GetAllUniqueTags())}");
        Console.WriteLine($"Спільні теги post-1 і post-2 (IntersectWith): {Format(manager.FindCommonTags("post-1", "post-2"))}");
        Console.WriteLine($"Теги post-1, яких немає в post-2 (ExceptWith): {Format(manager.FindUniqueTags("post-1", "post-2"))}");

        manager.RemoveTag("post-1", "dotnet");
        Console.WriteLine($"Теги post-1 після RemoveTag(\"dotnet\"): {Format(manager.GetTags("post-1"))}");
        Console.WriteLine();
    }

    private static void SearchIndexDemo()
    {
        Console.WriteLine("Сценарій 3. SearchIndex (Dictionary + HashSet)");

        SearchIndex index = new SearchIndex();
        index.AddDocument(1, "C# is a modern programming language");
        index.AddDocument(2, "Java is a popular programming language");
        index.AddDocument(3, "C# and Java are both object-oriented");

        Console.WriteLine($"Слів в індексі: {index.WordCount}");
        Console.WriteLine($"Search(\"programming\"): {Format(index.Search("programming"))}");
        Console.WriteLine($"Search(\"C#\"): {Format(index.Search("C#"))}");
        Console.WriteLine($"SearchMultiple(\"c#\", \"java\"): {Format(index.SearchMultiple("c#", "java"))}");
        Console.WriteLine($"SearchMultiple(\"programming\", \"modern\"): {Format(index.SearchMultiple("programming", "modern"))}");
        Console.WriteLine();
    }

    private static void ComparerDemo()
    {
        Console.WriteLine("Custom comparer");

        Dictionary<string, int> caseInsensitive = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        caseInsensitive["Hello"] = 1;
        Console.WriteLine($"caseInsensitive[\"HELLO\"] = {caseInsensitive["HELLO"]}");

        HashSet<Person> people = new HashSet<Person>(new PersonNameComparer());
        bool first = people.Add(new Person { Name = "John", Age = 25 });
        bool second = people.Add(new Person { Name = "John", Age = 30 });
        Console.WriteLine($"Додано John (25): {first}, додано John (30): {second}, усього людей: {people.Count}");
    }

    private static string Format<T>(IEnumerable<T> items)
    {
        return "{" + string.Join(", ", items.OrderBy(x => x)) + "}";
    }
}
