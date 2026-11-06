using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace IndependentWork17;
public class Program
{
    private static List<Product> _products = new List<Product>();
    private static List<Customer> _customers = new List<Customer>();
    private static List<Sale> _sales = new List<Sale>();

    // Скільки разів повторювати пайплайн для обчислення середнього "прогрітого" часу
    private const int WarmRuns = 50;

    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

        // Extract: "завантаження" даних
        Random random = new Random(2026);
        _products = DataGenerator.CreateProducts(random);
        _customers = DataGenerator.CreateCustomers(random, 80);
        _sales = DataGenerator.CreateSales(random, 5000, _products, _customers);

        Console.WriteLine($"Дані: товарів {_products.Count}, клієнтів {_customers.Count}, продажів {_sales.Count}");
        Console.WriteLine();

        // Transform + Load: пайплайни з вимірюванням часу
        PrintTopProducts(Measure("Пайплайн 1. Найпопулярніші продукти за категоріями", TopProductsByCategory));
        PrintCityRevenue(Measure("Пайплайн 2. Загальний дохід за містами клієнтів", RevenueByCity));
        PrintMonthlySales(Measure("Пайплайн 3. Продажі за місяцями та роками", SalesByMonth));
        PrintTopCustomers(Measure("Пайплайн 4. Найкращі клієнти", TopCustomers));
    }

    // Пайплайн 1: GroupBy -> Select(Sum) -> Join -> GroupBy -> OrderByDescending + Take(3)
    private static List<CategoryTopProducts> TopProductsByCategory()
    {
        return _sales
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(s => s.Quantity) })
            .Join(_products,
                  stat => stat.ProductId,
                  product => product.Id,
                  (stat, product) => new { product.Name, product.Category, stat.TotalQuantity })
            .GroupBy(x => x.Category)
            .Select(g => new CategoryTopProducts(
                g.Key,
                g.OrderByDescending(x => x.TotalQuantity)
                 .ThenBy(x => x.Name)
                 .Take(3)
                 .Select(x => new ProductQuantity(x.Name, x.TotalQuantity))
                 .ToList()))
            .OrderBy(c => c.Category)
            .ToList();
    }

    // Пайплайн 2: Join (товари) -> Join (клієнти) -> GroupBy(місто) -> Sum -> OrderByDescending
    private static List<CityRevenue> RevenueByCity()
    {
        return _sales
            .Join(_products, s => s.ProductId, p => p.Id,
                  (s, p) => new { s.CustomerId, Revenue = s.Quantity * p.UnitPrice })
            .Join(_customers, x => x.CustomerId, c => c.Id,
                  (x, c) => new { c.City, x.Revenue })
            .GroupBy(x => x.City)
            .Select(g => new CityRevenue(g.Key, g.Sum(x => x.Revenue), g.Count()))
            .OrderByDescending(c => c.Revenue)
            .ToList();
    }

    // Пайплайн 3: Join -> GroupBy(рік, місяць) -> Sum -> OrderBy(рік).ThenBy(місяць)
    private static List<MonthlySales> SalesByMonth()
    {
        return _sales
            .Join(_products, s => s.ProductId, p => p.Id,
                  (s, p) => new { s.SaleDate, s.Quantity, Revenue = s.Quantity * p.UnitPrice })
            .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
            .Select(g => new MonthlySales(
                g.Key.Year,
                g.Key.Month,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Revenue)))
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    // Пайплайн 4 (додатковий): топ-5 клієнтів за витратами з улюбленою категорією
    // Оператори: Where, Join, GroupBy, Select, Distinct, Aggregate, OrderByDescending, Take
    private static List<CustomerSummary> TopCustomers()
    {
        DateTime from = new DateTime(2026, 1, 1);

        return _sales
            .Where(s => s.SaleDate >= from)
            .Join(_products, s => s.ProductId, p => p.Id,
                  (s, p) => new { s.CustomerId, p.Category, Revenue = s.Quantity * p.UnitPrice })
            .GroupBy(x => x.CustomerId)
            .Join(_customers, g => g.Key, c => c.Id, (g, c) => new { Customer = c, Purchases = g.ToList() })
            .Select(x => new CustomerSummary(
                x.Customer.Name,
                x.Customer.City,
                x.Purchases.Sum(p => p.Revenue),
                x.Purchases.Count,
                x.Purchases.Select(p => p.Category).Distinct().Count(),
                // Aggregate: знаходимо категорію з найбільшими витратами за один прохід
                x.Purchases
                    .GroupBy(p => p.Category)
                    .Select(g => new { Category = g.Key, Spent = g.Sum(p => p.Revenue) })
                    .Aggregate((best, next) => next.Spent > best.Spent ? next : best)
                    .Category))
            .OrderByDescending(c => c.TotalSpent)
            .Take(5)
            .ToList();
    }

   
    private static T Measure<T>(string title, Func<T> pipeline)
    {
        Stopwatch coldWatch = Stopwatch.StartNew();
        T result = pipeline();
        coldWatch.Stop();

        Stopwatch warmWatch = Stopwatch.StartNew();
        for (int i = 0; i < WarmRuns; i++)
        {
            pipeline();
        }
        warmWatch.Stop();

        Console.WriteLine(title);
        Console.WriteLine($"   Час: перший запуск {coldWatch.Elapsed.TotalMilliseconds:F3} мс, " +
                          $"середній з {WarmRuns} повторних {warmWatch.Elapsed.TotalMilliseconds / WarmRuns:F3} мс");
        return result;
    }

    private static void PrintTopProducts(List<CategoryTopProducts> categories)
    {
        foreach (CategoryTopProducts category in categories)
        {
            Console.WriteLine($"   {category.Category}:");
            for (int i = 0; i < category.TopProducts.Count; i++)
            {
                ProductQuantity product = category.TopProducts[i];
                Console.WriteLine($"      {i + 1}. {product.Name} — {product.TotalQuantity} шт.");
            }
        }
        Console.WriteLine();
    }

    private static void PrintCityRevenue(List<CityRevenue> cities)
    {
        foreach (CityRevenue city in cities)
        {
            Console.WriteLine($"   {city.City,-12} {city.Revenue,15:N2} грн   (продажів: {city.SalesCount})");
        }
        Console.WriteLine($"   {"Разом",-12} {cities.Sum(c => c.Revenue),15:N2} грн");
        Console.WriteLine();
    }

    private static void PrintMonthlySales(List<MonthlySales> months)
    {
        foreach (MonthlySales month in months)
        {
            string name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Month);
            Console.WriteLine($"   {month.Year} {name,-9} {month.TotalUnits,5} од.  {month.Revenue,14:N2} грн");
        }
        Console.WriteLine();
    }

    private static void PrintTopCustomers(List<CustomerSummary> customers)
    {
        Console.WriteLine("   (покупки з 01.01.2026)");
        foreach (CustomerSummary c in customers)
        {
            Console.WriteLine($"   {c.Name} ({c.City}): {c.TotalSpent:N2} грн, покупок {c.OrdersCount}, " +
                              $"категорій {c.CategoriesCount}, улюблена — {c.FavoriteCategory}");
        }
        Console.WriteLine();
    }
}
