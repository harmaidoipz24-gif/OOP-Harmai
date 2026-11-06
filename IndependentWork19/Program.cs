using System.Diagnostics;
using System.Text;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

namespace IndependentWork19;

public class Program
{
    private static readonly Stopwatch Clock = Stopwatch.StartNew();

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        CustomExceptionDemo();
        await RetryScenarioAsync();
        CircuitBreakerScenario();
        await TimeoutRetryFallbackScenarioAsync();
    }

    // Логування з часом від запуску програми — видно реальні затримки між спробами
    public static void Log(string message)
    {
        Console.WriteLine($"[{Clock.ElapsedMilliseconds,6} мс] {message}");
    }

    // Частина 1. Власний тип винятку і try-catch-finally

    private static void CustomExceptionDemo()
    {
        Console.WriteLine("Частина 1. Власний виняток InsufficientStockException");
        Console.WriteLine();

        Warehouse warehouse = new Warehouse();
        warehouse.AddStock("Ноутбук", 3);
        warehouse.AddStock("Миша", 20);

        TryReserve(warehouse, "Миша", 5);
        TryReserve(warehouse, "Ноутбук", 5);
        TryReserve(warehouse, "Планшет", 1);
        TryReserve(warehouse, "Миша", 0);

        // Виняток з внутрішнім винятком: зберігаємо первинну причину
        try
        {
            try
            {
                warehouse.Reserve("Ноутбук", 10);
            }
            catch (InsufficientStockException ex)
            {
                throw new InvalidOperationException("Не вдалося оформити замовлення №1024", ex);
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Замовлення: {ex.Message}");
            Console.WriteLine($"   Причина (InnerException): {ex.InnerException?.Message}");
        }

        // Той самий сценарій через Result<T> — для очікуваних неуспіхів
        Console.WriteLine();
        Console.WriteLine("Перевірка наявності через Result<T> (без винятків):");
        foreach ((string product, int quantity) in new[] { ("Миша", 10), ("Ноутбук", 5), ("Планшет", 1) })
        {
            Result<int> result = warehouse.CheckAvailability(product, quantity);
            Console.WriteLine(result.IsSuccess
                ? $"   {product} × {quantity}: можна купити (на складі {result.Value})"
                : $"   {product} × {quantity}: не можна — {result.Error}");
        }
        Console.WriteLine();
    }

    private static void TryReserve(Warehouse warehouse, string product, int quantity)
    {
        Console.WriteLine($"Резервування: {product} × {quantity}");
        try
        {
            warehouse.Reserve(product, quantity);
            Console.WriteLine("   Успішно зарезервовано");
        }
        // Від найспецифічнішого до найзагальнішого
        catch (InsufficientStockException ex)
        {
            Console.WriteLine($"   InsufficientStockException: {ex.Message}");
            Console.WriteLine($"   Запитано {ex.RequestedQuantity}, є {ex.AvailableQuantity}, не вистачає {ex.Shortage} — пропонуємо передзамовлення");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"   KeyNotFoundException: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   {ex.GetType().Name}: некоректна кількість ({ex.ParamName})");
        }
        finally
        {
            Console.WriteLine("   Операцію резервування завершено (finally)");
        }
    }

    // Сценарій 1. Retry з експоненційною затримкою та jitter

    private static async Task RetryScenarioAsync()
    {
        Console.WriteLine("Сценарій 1. Retry: тимчасово недоступний API курсу валют");
        Console.WriteLine();

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                // Повторюємо лише тимчасові мережеві помилки
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    Log($"Retry #{args.AttemptNumber + 1} через {args.RetryDelay.TotalMilliseconds:F0} мс: {args.Outcome.Exception?.Message}");
                    return default;
                }
            })
            .Build();

        // а) API двічі відповідає 503, на третій раз — успішно
        ExchangeRateApi api = new ExchangeRateApi(failuresBeforeSuccess: 2);
        try
        {
            decimal rate = await pipeline.ExecuteAsync(token => api.GetUsdRateAsync(token));
            Log($"Успіх: курс USD = {rate} грн (усього запитів: {api.CallCount})");
        }
        catch (HttpRequestException ex)
        {
            Log($"Остаточна невдача: {ex.Message}");
        }
        Console.WriteLine();

        // б) API "лежить" довше, ніж дозволяє політика: після 1 + 4 спроб виняток передається далі
        ExchangeRateApi brokenApi = new ExchangeRateApi(failuresBeforeSuccess: 10);
        try
        {
            await pipeline.ExecuteAsync(token => brokenApi.GetUsdRateAsync(token));
        }
        catch (HttpRequestException ex)
        {
            Log($"Остаточна невдача після {brokenApi.CallCount} запитів: {ex.Message}. Показуємо курс з учорашнього кешу.");
        }
        Console.WriteLine();

        // в) Постійна помилка (401) не потрапляє під ShouldHandle — жодних повторів
        UnauthorizedApi unauthorizedApi = new UnauthorizedApi();
        try
        {
            await pipeline.ExecuteAsync(token => unauthorizedApi.GetUsdRateAsync(token));
        }
        catch (UnauthorizedAccessException ex)
        {
            Log($"Без повторів ({unauthorizedApi.CallCount} запит): {ex.Message}");
        }
        Console.WriteLine();
    }

    // Сценарій 2. Circuit Breaker для бази даних, що "впала"

    private static void CircuitBreakerScenario()
    {
        Console.WriteLine("Сценарій 2. Circuit Breaker: база даних замовлень недоступна");
        Console.WriteLine();

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutException>(),
                FailureRatio = 0.5,                          // розімкнути, якщо невдалих ≥ 50%
                MinimumThroughput = 4,                       // ... серед щонайменше 4 запитів
                SamplingDuration = TimeSpan.FromSeconds(10), // ... за останні 10 секунд
                BreakDuration = TimeSpan.FromSeconds(1),     // не звертатися до БД 1 секунду
                OnOpened = args =>
                {
                    Log($"Circuit OPEN: запити блокуються на {args.BreakDuration.TotalSeconds:F0} с");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    Log("Circuit HALF-OPEN: пробний запит до БД");
                    return default;
                },
                OnClosed = args =>
                {
                    Log("Circuit CLOSED: БД відновилась, робота в звичайному режимі");
                    return default;
                }
            })
            .Build();

        OrdersDatabase database = new OrdersDatabase { IsDown = true };

        for (int request = 1; request <= 8; request++)
        {
            if (request == 7)
            {
                database.IsDown = false;
                Log("Адміністратор перезапустив сервер БД. Чекаємо завершення BreakDuration...");
                Thread.Sleep(1100);
            }

            try
            {
                int count = pipeline.Execute(() => database.GetPendingOrdersCount());
                Log($"Запит {request}: замовлень в обробці — {count}");
            }
            catch (BrokenCircuitException)
            {
                Log($"Запит {request}: відхилено миттєво, БД не навантажується");
            }
            catch (TimeoutException ex)
            {
                Log($"Запит {request}: помилка — {ex.Message}");
            }
        }

        Log($"Запитів від застосунку: 8, реально дійшло до БД: {database.RealCalls}");
        Console.WriteLine();
    }

    // Сценарій 3. Fallback + Retry + Timeout для повільного сервісу

    private static async Task TimeoutRetryFallbackScenarioAsync()
    {
        Console.WriteLine("Сценарій 3. Timeout + Retry + Fallback: повільний сервіс прогнозу доставки");
        Console.WriteLine();

        // Порядок важливий: перша додана стратегія — зовнішня.
        // Fallback охоплює все; Retry повторює спроби; Timeout обмежує КОЖНУ спробу окремо.
        ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
            .AddFallback(new FallbackStrategyOptions<string>
            {
                ShouldHandle = new PredicateBuilder<string>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<HttpRequestException>(),
                FallbackAction = args => Outcome.FromResultAsValueTask("орієнтовно 2–4 дні (типовий термін, сервіс недоступний)"),
                OnFallback = args =>
                {
                    Log($"Fallback: усі спроби невдалі ({args.Outcome.Exception?.GetType().Name}), повертаємо типовий термін");
                    return default;
                }
            })
            .AddRetry(new RetryStrategyOptions<string>
            {
                ShouldHandle = new PredicateBuilder<string>().Handle<TimeoutRejectedException>(),
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Constant,
                OnRetry = args =>
                {
                    Log($"Retry #{args.AttemptNumber + 1} через {args.RetryDelay.TotalMilliseconds:F0} мс");
                    return default;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromMilliseconds(300),
                OnTimeout = args =>
                {
                    Log($"Timeout: спробу перервано після {args.Timeout.TotalMilliseconds:F0} мс");
                    return default;
                }
            })
            .Build();

        // а) Перша відповідь повільна (800 мс), друга — швидка (120 мс)
        DeliveryEstimateService service = new DeliveryEstimateService(800, 120);
        string estimate = await pipeline.ExecuteAsync(token => service.GetEstimateAsync("Рівне", token));
        Log($"Результат: {estimate}");
        Console.WriteLine();

        // б) Сервіс завжди відповідає повільно — користувач все одно отримує відповідь, а не помилку
        DeliveryEstimateService slowService = new DeliveryEstimateService(800, 900, 1000);
        string fallbackEstimate = await pipeline.ExecuteAsync(token => slowService.GetEstimateAsync("Львів", token));
        Log($"Результат: {fallbackEstimate}");
        Console.WriteLine();
    }
}
