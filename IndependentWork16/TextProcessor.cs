using System.Text.RegularExpressions;

namespace IndependentWork16;

/// <summary>
/// Спільна нормалізація тексту. Її використовують і інвертований індекс, і лінійний пошук,
/// щоб порівняння продуктивності було коректним.
/// </summary>
public static class TextProcessor
{
    // Літери, цифри, '_' а також '#' і '+', щоб "C#" і "C++" не перетворювались на "c"
    private static readonly Regex WordRegex = new Regex(@"[\w#+]+", RegexOptions.Compiled);

    private static readonly HashSet<string> StopWords = new HashSet<string>
    {
        "a", "an", "the", "is", "are", "was", "were",
        "in", "on", "at", "to", "for", "of", "and", "or"
    };

    /// <summary>
    /// Розбиває текст на слова, видаляє пунктуацію, переводить у нижній регістр.
    /// Приклад: "Hello, world!" -> ["hello", "world"]
    /// </summary>
    public static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        foreach (Match match in WordRegex.Matches(text.ToLowerInvariant()))
        {
            yield return match.Value;
        }
    }

    /// <summary>Видаляє стоп-слова (a, an, the, is, are, ...).</summary>
    public static IEnumerable<string> RemoveStopWords(IEnumerable<string> words)
    {
        return words.Where(w => !StopWords.Contains(w));
    }

    /// <summary>Повний конвеєр нормалізації: токенізація + видалення стоп-слів.</summary>
    public static IEnumerable<string> GetTerms(string text)
    {
        return RemoveStopWords(Tokenize(text));
    }

    /// <summary>Нормалізує одне слово запиту так само, як слова документів.</summary>
    public static string? NormalizeWord(string word)
    {
        return Tokenize(word).FirstOrDefault();
    }
}
