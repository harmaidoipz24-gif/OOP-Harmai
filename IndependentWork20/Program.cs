using System.Text;

namespace IndependentWork20;

public class GradebookServer
{
    private readonly int _failuresBeforeSuccess;
    public int Calls { get; private set; }

    public GradebookServer(int failuresBeforeSuccess)
    {
        _failuresBeforeSuccess = failuresBeforeSuccess;
    }

    public double GetAverageGrade(string student)
    {
        Calls++;
        if (Calls <= _failuresBeforeSuccess)
            throw new TimeoutException("Сервер журналу не відповів за 5 с");

        if (string.IsNullOrWhiteSpace(student))
            throw new ArgumentException("Не вказано ПІБ студента", nameof(student));

        return 4.6;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Обрані параметри: 4 повторні спроби, початкова затримка 250 мс, jitter ±20%
        RetryPolicy policy = new RetryPolicy(maxRetries: 4, initialDelay: TimeSpan.FromMilliseconds(250), jitterFactor: 0.2);

        DelayTableDemo(policy);
        SuccessAfterRetriesDemo(policy);
        AlwaysFailsDemo(policy);
        ShouldRetryDemo(policy);
    }

    // Як зростає затримка і як jitter розкидає її значення
    private static void DelayTableDemo(RetryPolicy policy)
    {
        Console.WriteLine("Розрахунок затримок: initialDelay = 250 мс, jitterFactor = 0.2");
        Console.WriteLine("   Повтор | Без jitter | Три випадкові значення з jitter");
        for (int attempt = 0; attempt < policy.MaxRetries; attempt++)
        {
            double baseDelay = 250 * Math.Pow(2, attempt);
            string samples = string.Join(", ", Enumerable.Range(0, 3)
                .Select(_ => $"{policy.CalculateDelay(attempt).TotalMilliseconds:F0}"));
            Console.WriteLine($"   {attempt + 1,6} | {baseDelay,7:F0} мс | {samples} мс (межі {baseDelay * 0.8:F0}–{baseDelay * 1.2:F0})");
        }
        Console.WriteLine();
    }

    // Сценарій 1: операція спочатку падає, потім успішна
    private static void SuccessAfterRetriesDemo(RetryPolicy policy)
    {
        Console.WriteLine("Сценарій 1. Сервер журналу відповідає з третьої спроби");

        GradebookServer server = new GradebookServer(failuresBeforeSuccess: 2);
        double grade = policy.Execute(() => server.GetAverageGrade("Гармай Денис"));

        Console.WriteLine($"Результат: середній бал = {grade} (запитів до сервера: {server.Calls})");
        Console.WriteLine();
    }

    // Сценарій 2: операція завжди падає — після всіх спроб політика передає виняток далі
    private static void AlwaysFailsDemo(RetryPolicy policy)
    {
        Console.WriteLine("Сценарій 2. Сервер недоступний постійно");

        GradebookServer server = new GradebookServer(failuresBeforeSuccess: int.MaxValue);
        try
        {
            policy.Execute(() => server.GetAverageGrade("Гармай Денис"));
            Console.WriteLine("Несподіваний успіх");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Остаточна невдача після {server.Calls} запитів: {ex.Message}");
            Console.WriteLine("Показуємо користувачу: \"Журнал тимчасово недоступний, спробуйте пізніше\"");
        }
        Console.WriteLine();
    }

    // Сценарій 3: shouldRetry — повторюємо лише тимчасові збої
    private static void ShouldRetryDemo(RetryPolicy policy)
    {
        Console.WriteLine("Сценарій 3. shouldRetry: помилка в запиті не повторюється");

        static bool IsTransient(Exception ex) => ex is TimeoutException or HttpRequestException or IOException;

        GradebookServer server = new GradebookServer(failuresBeforeSuccess: 1);
        try
        {
            // Перший збій (TimeoutException) — тимчасовий, повторюємо.
            // Другий (ArgumentException, порожнє ім'я) — помилка самого запиту, повтор нічого не змінить.
            policy.Execute(() => server.GetAverageGrade(""), IsTransient);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Запит некоректний ({ex.Message}). Запитів до сервера: {server.Calls} — зайвих повторів не було");
        }
        Console.WriteLine();
    }
}
