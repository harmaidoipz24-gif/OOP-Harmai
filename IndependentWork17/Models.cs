namespace IndependentWork17;

// Сутності предметної області "Продажі"

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class Sale
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime SaleDate { get; set; }
    public int CustomerId { get; set; }
}

// Результати пайплайнів

public record ProductQuantity(string Name, int TotalQuantity);

public record CategoryTopProducts(string Category, List<ProductQuantity> TopProducts);

public record CityRevenue(string City, decimal Revenue, int SalesCount);

public record MonthlySales(int Year, int Month, int TotalUnits, decimal Revenue);

public record CustomerSummary(string Name, string City, decimal TotalSpent, int OrdersCount, int CategoriesCount, string FavoriteCategory);
