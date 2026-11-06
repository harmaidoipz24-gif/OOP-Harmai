using System.Diagnostics;

namespace IndependentWork16;

public record PerformanceResult(
    int DocumentCount,
    double LinearMicroseconds,
    long LinearTicks,
    double IndexMicroseconds,
    long IndexTicks,
    double BuildMilliseconds,
    int LinearFound,
    int IndexFound)
{
    public double Speedup => IndexMicroseconds > 0 ? LinearMicroseconds / IndexMicroseconds : double.PositiveInfinity;

    // Скільки пошуків треба виконати, щоб час побудови індексу окупився
    public double BreakEvenQueries
    {
        get
        {
            double savedPerQuery = LinearMicroseconds - IndexMicroseconds;
            return savedPerQuery > 0 ? BuildMilliseconds * 1000 / savedPerQuery : double.PositiveInfinity;
        }
    }
}

public static class PerformanceTest
{
    /// <summary>
    /// Порівнює лінійний пошук і пошук через інвертований індекс.
    /// Обидва підходи використовують однакову нормалізацію (TextProcessor.GetTerms),
    /// тому знаходять однакову кількість документів.
    /// </summary>
    public static PerformanceResult CompareSearchMethods(List<Document> documents, string searchWord)
    {
        string normalizedWord = TextProcessor.NormalizeWord(searchWord) ?? string.Empty;

        // Один вимір триває мікросекунди, тому повторюємо пошук багато разів і беремо середнє
        int linearRepeats = Math.Max(5, 20_000 / documents.Count);
        int indexRepeats = 100_000;

        // Прогрів (JIT-компіляція), щоб перший виклик не спотворив результат
        LinearSearch(documents, normalizedWord);

        // 1. Лінійний пошук: для кожного запиту токенізуємо всі документи
        Stopwatch sw = Stopwatch.StartNew();
        List<Document> linearResults = new List<Document>();
        for (int i = 0; i < linearRepeats; i++)
        {
            linearResults = LinearSearch(documents, normalizedWord);
        }
        sw.Stop();
        long linearTicks = sw.ElapsedTicks / linearRepeats;
        double linearMicroseconds = sw.Elapsed.TotalMicroseconds / linearRepeats;

        // 2. Побудова індексу (виконується один раз)
        sw.Restart();
        InvertedIndex index = new InvertedIndex();
        foreach (Document doc in documents)
        {
            index.AddDocument(doc);
        }
        sw.Stop();
        double buildMilliseconds = sw.Elapsed.TotalMilliseconds;

        // 3. Пошук через індекс
        index.SearchSingleWord(searchWord); // прогрів
        sw.Restart();
        List<Document> indexResults = new List<Document>();
        for (int i = 0; i < indexRepeats; i++)
        {
            indexResults = index.SearchSingleWord(searchWord);
        }
        sw.Stop();
        long indexTicks = sw.ElapsedTicks / indexRepeats;
        double indexMicroseconds = sw.Elapsed.TotalMicroseconds / indexRepeats;

        return new PerformanceResult(
            documents.Count,
            linearMicroseconds, linearTicks,
            indexMicroseconds, indexTicks,
            buildMilliseconds,
            linearResults.Count, indexResults.Count);
    }

    private static List<Document> LinearSearch(List<Document> documents, string normalizedWord)
    {
        return documents
            .Where(d => TextProcessor.GetTerms(d.Content).Contains(normalizedWord))
            .ToList();
    }
}
