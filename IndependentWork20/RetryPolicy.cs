namespace IndependentWork20;

public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _jitterFactor;

    public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor = 0.2)
    {
        if (maxRetries < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Кількість повторів не може бути від'ємною.");
        if (initialDelay <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(initialDelay), "Початкова затримка має бути додатною.");
        if (jitterFactor < 0 || jitterFactor >= 1)
            throw new ArgumentOutOfRangeException(nameof(jitterFactor), "Коефіцієнт jitter має бути в межах [0; 1).");

        _maxRetries = maxRetries;
        _initialDelay = initialDelay;
        _jitterFactor = jitterFactor;
    }

    public int MaxRetries => _maxRetries;

    public T Execute<T>(Func<T> operation, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // attempt = 0 — перша спроба, 1.._maxRetries — повторні
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                Log($"Спроба {attempt + 1} з {_maxRetries + 1}");
                T result = operation();
                Log($"Спроба {attempt + 1}: успіх");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Спроба {attempt + 1}: невдача — {ex.GetType().Name}: {ex.Message}");

                bool retryable = shouldRetry?.Invoke(ex) ?? true;
                if (!retryable)
                {
                    Log("Виняток не підлягає повтору (shouldRetry = false) — передаємо його далі");
                    throw;
                }

                if (attempt >= _maxRetries)
                {
                    Log($"Усі {_maxRetries} повторні спроби вичерпано — передаємо виняток далі");
                    throw;
                }

                TimeSpan delay = CalculateDelay(attempt);
                Log($"Очікування {delay.TotalMilliseconds:F0} мс перед повторною спробою №{attempt + 1}");
                Thread.Sleep(delay);
            }
        }
    }

    public TimeSpan CalculateDelay(int retryAttempt)
    {
        double jitter = 1 + Random.Shared.NextDouble() * _jitterFactor * 2 - _jitterFactor;
        return _initialDelay * Math.Pow(2, retryAttempt) * jitter;
    }

    private static void Log(string message)
    {
        Console.WriteLine($"   [{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
}
