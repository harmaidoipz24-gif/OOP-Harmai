namespace IndependentWork19;

public class ExchangeRateApi
{
    private readonly int _failuresBeforeSuccess;
    public int CallCount { get; private set; }

    public ExchangeRateApi(int failuresBeforeSuccess)
    {
        _failuresBeforeSuccess = failuresBeforeSuccess;
    }

    public async ValueTask<decimal> GetUsdRateAsync(CancellationToken token)
    {
        CallCount++;
        await Task.Delay(50, token); // мережева затримка
        Program.Log($"   API: запит #{CallCount}");

        if (CallCount <= _failuresBeforeSuccess)
            throw new HttpRequestException("503 Service Unavailable");

        return 41.27m;
    }
}


public class UnauthorizedApi
{
    public int CallCount { get; private set; }

    public ValueTask<decimal> GetUsdRateAsync(CancellationToken token)
    {
        CallCount++;
        Program.Log($"   API: запит #{CallCount}");
        throw new UnauthorizedAccessException("401 Unauthorized: недійсний API-ключ");
    }
}

public class OrdersDatabase
{
    public bool IsDown { get; set; }
    public int RealCalls { get; private set; }

    public int GetPendingOrdersCount()
    {
        RealCalls++;
        if (IsDown)
            throw new TimeoutException("Не вдалося під'єднатися до БД: час очікування з'єднання вичерпано");

        return 17;
    }
}
public class DeliveryEstimateService
{
    private readonly int[] _responseDelaysMs;
    private int _attempt;

    public DeliveryEstimateService(params int[] responseDelaysMs)
    {
        _responseDelaysMs = responseDelaysMs;
    }

    public async ValueTask<string> GetEstimateAsync(string city, CancellationToken token)
    {
        int delay = _responseDelaysMs[Math.Min(_attempt, _responseDelaysMs.Length - 1)];
        _attempt++;
        Program.Log($"   Сервіс доставки: спроба #{_attempt}, відповідь займе {delay} мс");

        // Task.Delay з токеном: коли Timeout-стратегія скасує операцію, очікування перерветься
        await Task.Delay(delay, token);
        return $"доставка до м. {city}: 2 дні";
    }
}
