namespace IndependentWork19;
public class InsufficientStockException : Exception
{
    public string ProductName { get; } = string.Empty;
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    // Три стандартні конструктори
    public InsufficientStockException()
        : base("Недостатньо товару на складі.")
    {
    }

    public InsufficientStockException(string message)
        : base(message)
    {
    }

    public InsufficientStockException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    // Конструктор з контекстом помилки
    public InsufficientStockException(string productName, int requestedQuantity, int availableQuantity)
        : base($"Неможливо зарезервувати {requestedQuantity} шт. товару \"{productName}\": у наявності лише {availableQuantity} шт.")
    {
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }

    public int Shortage => RequestedQuantity - AvailableQuantity;
}


public record Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure(string error) => new Result<T>(false, default, error);
}

public class Warehouse
{
    private readonly Dictionary<string, int> _stock = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public void AddStock(string productName, int quantity)
    {
        _stock[productName] = _stock.GetValueOrDefault(productName) + quantity;
    }


    public void Reserve(string productName, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (!_stock.TryGetValue(productName, out int available))
            throw new KeyNotFoundException($"Товар \"{productName}\" відсутній у каталозі складу.");

        if (available < quantity)
            throw new InsufficientStockException(productName, quantity, available);

        _stock[productName] = available - quantity;
    }


    public Result<int> CheckAvailability(string productName, int quantity)
    {
        if (!_stock.TryGetValue(productName, out int available))
            return Result<int>.Failure($"Товар \"{productName}\" не знайдено");

        return available >= quantity
            ? Result<int>.Success(available)
            : Result<int>.Failure($"Доступно лише {available} шт.");
    }
}
