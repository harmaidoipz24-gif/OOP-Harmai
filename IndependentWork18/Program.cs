using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace IndependentWork18;
public class Program
{
    private const int MaxValue = 5_000_000;

    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

        Console.WriteLine($"Логічних процесорів: {Environment.ProcessorCount}");
        Console.WriteLine($"Конфігурація: {(IsDebugBuild() ? "Debug (для чесних вимірювань запускайте -c Release)" : "Release")}");
        Console.WriteLine();

        WarmUp();

        Console.WriteLine("Частина 1. Продуктивність LINQ і PLINQ");
        Console.WriteLine();

        List<(int Size, string Operation, double LinqMs, double PlinqMs)> results = new();

        foreach (int size in new[] { 1_000_000, 5_000_000, 10_000_000 })
        {
            List<int> data = GenerateData(size);
            Console.WriteLine($"Колекція: {size:N0} елементів");

            // Обчислювально інтенсивна операція: перевірка на просте число + математичне перетворення
            (double linqHeavy, double plinqHeavy) = Compare(
                "Важка операція (IsPrime + Sqrt*Log)",
                () => data.Where(IsPrime).Select(Transform).Sum(),
                () => data.AsParallel().Where(IsPrime).Select(Transform).Sum());
            results.Add((size, "важка", linqHeavy, plinqHeavy));

            // Дешева операція — для порівняння, коли паралелізм не окупається
            (double linqLight, double plinqLight) = Compare(
                "Дешева операція (n % 3 == 0, n * 2)",
                () => data.Where(n => n % 3 == 0).Select(n => (long)n * 2).Sum(),
                () => data.AsParallel().Where(n => n % 3 == 0).Select(n => (long)n * 2).Sum());
            results.Add((size, "дешева", linqLight, plinqLight));

            Console.WriteLine();
        }

        PrintTable(results);

        Console.WriteLine("Частина 2. Побічні ефекти в PLINQ");
        Console.WriteLine();
        SideEffectsDemo();
    }

    // Перевірка на просте число методом перебору дільників до √n
    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n % 2 == 0) return n == 2;

        int limit = (int)Math.Sqrt(n);
        for (int divisor = 3; divisor <= limit; divisor += 2)
        {
            if (n % divisor == 0)
                return false;
        }
        return true;
    }

    private static double Transform(int n) => Math.Sqrt(n) * Math.Log(n);

    private static List<int> GenerateData(int size)
    {
        Random random = new Random(12); // фіксований seed — однакові дані для LINQ і PLINQ
        List<int> data = new List<int>(size);
        for (int i = 0; i < size; i++)
        {
            data.Add(random.Next(1, MaxValue));
        }
        return data;
    }

       private static (double LinqMs, double PlinqMs) Compare<T>(string title, Func<T> linq, Func<T> plinq)
    {
        Stopwatch linqWatch = Stopwatch.StartNew();
        T linqResult = linq();
        linqWatch.Stop();

        Stopwatch plinqWatch = Stopwatch.StartNew();
        T plinqResult = plinq();
        plinqWatch.Stop();

        double linqMs = linqWatch.Elapsed.TotalMilliseconds;
        double plinqMs = plinqWatch.Elapsed.TotalMilliseconds;
        bool same = ResultsEqual(linqResult, plinqResult);

        Console.WriteLine($"   {title}");
        Console.WriteLine($"      LINQ:  {linqMs,10:F1} мс");
        Console.WriteLine($"      PLINQ: {plinqMs,10:F1} мс   прискорення ×{linqMs / plinqMs:F2}");
        Console.WriteLine($"      Результати однакові: {(same ? "так" : "НІ")} ({linqResult})");

        return (linqMs, plinqMs);
    }

    // Суму double у паралельному режимі додають в іншому порядку, тому можлива різниця в останніх знаках
    private static bool ResultsEqual<T>(T a, T b)
    {
        if (a is double x && b is double y)
            return Math.Abs(x - y) <= Math.Abs(x) * 1e-9;
        return EqualityComparer<T>.Default.Equals(a, b);
    }

    // Прогрів: JIT-компіляція і запуск пулу потоків до початку вимірювань
    private static void WarmUp()
    {
        List<int> small = GenerateData(100_000);
        small.Where(IsPrime).Select(Transform).Sum();
        small.AsParallel().Where(IsPrime).Select(Transform).Sum();
        small.Where(n => n % 3 == 0).Select(n => (long)n * 2).Sum();
        small.AsParallel().Where(n => n % 3 == 0).Select(n => (long)n * 2).Sum();
    }

    private static void PrintTable(List<(int Size, string Operation, double LinqMs, double PlinqMs)> results)
    {
        Console.WriteLine("Таблиця для REPORT.md:");
        Console.WriteLine("| Розмір | Операція | LINQ, мс | PLINQ, мс | Прискорення |");
        Console.WriteLine("|---|---|---|---|---|");
        foreach ((int size, string operation, double linqMs, double plinqMs) in results)
        {
            Console.WriteLine($"| {size:N0} | {operation} | {linqMs:F1} | {plinqMs:F1} | ×{linqMs / plinqMs:F2} |");
        }
        Console.WriteLine();
    }

    private static void SideEffectsDemo()
    {
        List<int> data = GenerateData(1_000_000);
        int expected = data.Count(n => n % 2 == 0);
        Console.WriteLine($"Завдання: порахувати парні числа в колекції з {data.Count:N0} елементів. Правильна відповідь: {expected:N0}");
        Console.WriteLine();

        // ПРОБЛЕМА 1: спільний лічильник змінюється з кількох потоків без синхронізації
        Console.WriteLine("Проблема 1: counter++ всередині PLINQ без синхронізації");
        for (int run = 1; run <= 3; run++)
        {
            int unsafeCounter = 0;
            data.AsParallel().ForAll(n =>
            {
                if (n % 2 == 0)
                    unsafeCounter++; // читання, збільшення і запис — три окремі кроки, потоки перезаписують один одного
            });
            Console.WriteLine($"   Запуск {run}: {unsafeCounter:N0} (втрачено {expected - unsafeCounter:N0})");
        }
        Console.WriteLine();

        // ПРОБЛЕМА 2: List<T> не потокобезпечний
        Console.WriteLine("Проблема 2: List<int>.Add з кількох потоків");
        for (int run = 1; run <= 3; run++)
        {
            List<int> unsafeList = new List<int>();
            try
            {
                data.AsParallel().ForAll(n =>
                {
                    if (n % 2 == 0)
                        unsafeList.Add(n);
                });
                Console.WriteLine($"   Запуск {run}: у списку {unsafeList.Count:N0} елементів (очікувалось {expected:N0})");
            }
            catch (AggregateException ex)
            {
                Exception inner = ex.InnerExceptions[0];
                Console.WriteLine($"   Запуск {run}: виняток {inner.GetType().Name}: {inner.Message}");
            }
        }
        Console.WriteLine();

        // ВИПРАВЛЕННЯ
        Console.WriteLine("Виправлення");

        object sync = new object();
        int lockedCounter = 0;
        Stopwatch sw = Stopwatch.StartNew();
        data.AsParallel().ForAll(n =>
        {
            if (n % 2 == 0)
            {
                lock (sync)
                {
                    lockedCounter++;
                }
            }
        });
        sw.Stop();
        Console.WriteLine($"   lock:              {lockedCounter:N0}  {Check(lockedCounter, expected)}  ({sw.Elapsed.TotalMilliseconds:F1} мс)");

        int interlockedCounter = 0;
        sw.Restart();
        data.AsParallel().ForAll(n =>
        {
            if (n % 2 == 0)
                Interlocked.Increment(ref interlockedCounter);
        });
        sw.Stop();
        Console.WriteLine($"   Interlocked:       {interlockedCounter:N0}  {Check(interlockedCounter, expected)}  ({sw.Elapsed.TotalMilliseconds:F1} мс)");

        ConcurrentBag<int> bag = new ConcurrentBag<int>();
        sw.Restart();
        data.AsParallel().ForAll(n =>
        {
            if (n % 2 == 0)
                bag.Add(n);
        });
        sw.Stop();
        Console.WriteLine($"   ConcurrentBag:     {bag.Count:N0}  {Check(bag.Count, expected)}  ({sw.Elapsed.TotalMilliseconds:F1} мс)");

        sw.Restart();
        int pureCount = data.AsParallel().Count(n => n % 2 == 0);
        sw.Stop();
        Console.WriteLine($"   Count() без побічних ефектів: {pureCount:N0}  {Check(pureCount, expected)}  ({sw.Elapsed.TotalMilliseconds:F1} мс)");

        sw.Restart();
        List<int> pureList = data.AsParallel().Where(n => n % 2 == 0).ToList();
        sw.Stop();
        Console.WriteLine($"   Where().ToList() без побічних ефектів: {pureList.Count:N0}  {Check(pureList.Count, expected)}  ({sw.Elapsed.TotalMilliseconds:F1} мс)");
    }

    private static string Check(int actual, int expected) => actual == expected ? "правильно" : "НЕПРАВИЛЬНО";

    private static bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
