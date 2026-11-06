namespace IndependentWork16;

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public override string ToString() => $"[{Id}] {Title}";
}

/// <summary>
/// Інвертований індекс: замість "документ -> слова" зберігає "слово -> документи".
/// </summary>
public class InvertedIndex
{
    // Слово -> множина ID документів
    private readonly Dictionary<string, HashSet<int>> _index;

    // ID -> документ (для швидкого отримання)
    private readonly Dictionary<int, Document> _documents;

    public InvertedIndex()
    {
        _index = new Dictionary<string, HashSet<int>>();
        _documents = new Dictionary<int, Document>();
    }

    public int DocumentCount => _documents.Count;
    public int UniqueWords => _index.Keys.Count;

    /// <summary>Додає документ до індексу.</summary>
    public void AddDocument(Document doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        // 1. Зберегти документ
        _documents[doc.Id] = doc;

        // 2. Розбити вміст на нормалізовані слова
        // 3. Для кожного слова додати Id документа; HashSet ігнорує повтори слова в одному документі
        foreach (string word in TextProcessor.GetTerms(doc.Content))
        {
            if (!_index.TryGetValue(word, out HashSet<int>? docIds))
            {
                docIds = new HashSet<int>();
                _index[word] = docIds;
            }
            docIds.Add(doc.Id);
        }
    }

    /// <summary>Шукає документи, що містять слово.</summary>
    public List<Document> SearchSingleWord(string word)
    {
        return ToDocuments(GetDocumentIds(word));
    }

    /// <summary>Шукає документи, що містять УСІ слова (AND).</summary>
    public List<Document> SearchMultipleWords(params string[] words)
    {
        if (words.Length == 0)
            return new List<Document>();

        // Копія множини першого слова, щоб IntersectWith не зіпсував сам індекс
        HashSet<int> result = new HashSet<int>(GetDocumentIds(words[0]));

        for (int i = 1; i < words.Length && result.Count > 0; i++)
        {
            result.IntersectWith(GetDocumentIds(words[i]));
        }

        return ToDocuments(result);
    }

    /// <summary>Шукає документи, що містять БУДЬ-ЯКЕ слово (OR).</summary>
    public List<Document> SearchAnyWord(params string[] words)
    {
        HashSet<int> result = new HashSet<int>();

        foreach (string word in words)
        {
            result.UnionWith(GetDocumentIds(word));
        }

        return ToDocuments(result);
    }

    /// <summary>
    /// Додатково: ранжований OR-пошук — документи сортуються за кількістю знайдених слів запиту.
    /// </summary>
    public List<(Document Document, int Matches)> SearchRanked(params string[] words)
    {
        Dictionary<int, int> matches = new Dictionary<int, int>();

        foreach (string word in words.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            foreach (int id in GetDocumentIds(word))
            {
                matches[id] = matches.GetValueOrDefault(id) + 1;
            }
        }

        return matches
            .OrderByDescending(m => m.Value)
            .ThenBy(m => m.Key)
            .Select(m => (_documents[m.Key], m.Value))
            .ToList();
    }

    private IReadOnlySet<int> GetDocumentIds(string word)
    {
        string? normalized = TextProcessor.NormalizeWord(word);

        if (normalized != null && _index.TryGetValue(normalized, out HashSet<int>? ids))
            return ids;

        return EmptySet;
    }

    private List<Document> ToDocuments(IEnumerable<int> ids)
    {
        return ids.OrderBy(id => id).Select(id => _documents[id]).ToList();
    }

    private static readonly HashSet<int> EmptySet = new HashSet<int>();
}
