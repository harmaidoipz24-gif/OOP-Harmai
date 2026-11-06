namespace IndependentWork17;
public static class DataGenerator
{
    private static readonly Dictionary<string, (string Name, decimal MinPrice, decimal MaxPrice)[]> Catalog = new()
    {
        ["Електроніка"] = new[]
        {
            ("Ноутбук", 18000m, 45000m), ("Смартфон", 7000m, 35000m), ("Навушники", 600m, 6000m),
            ("Монітор", 4500m, 15000m), ("Клавіатура", 500m, 3500m), ("Миша", 300m, 2500m),
            ("Павербанк", 600m, 2000m), ("Флешка", 200m, 900m), ("Вебкамера", 900m, 4000m),
            ("Роутер", 1000m, 5000m)
        },
        ["Книги"] = new[]
        {
            ("Чистий код", 400m, 700m), ("CLR via C#", 900m, 1500m), ("Патерни проєктування", 500m, 900m),
            ("Кобзар", 200m, 600m), ("Тигролови", 200m, 450m), ("Алгоритми", 700m, 1300m),
            ("Рефакторинг", 500m, 950m), ("Мистецтво програмування", 1200m, 2500m),
            ("Чорна рада", 180m, 400m), ("Лісова пісня", 150m, 350m)
        },
        ["Одяг"] = new[]
        {
            ("Футболка", 300m, 900m), ("Худі", 900m, 2500m), ("Джинси", 1000m, 3000m),
            ("Куртка", 2500m, 7000m), ("Кросівки", 1800m, 5500m), ("Шапка", 250m, 700m),
            ("Шарф", 300m, 900m), ("Сорочка", 700m, 1800m), ("Шорти", 400m, 1100m),
            ("Шкарпетки", 60m, 200m)
        },
        ["Спорт"] = new[]
        {
            ("М'яч футбольний", 500m, 2500m), ("Гантелі", 700m, 3000m), ("Килимок для йоги", 400m, 1200m),
            ("Скакалка", 150m, 500m), ("Велошолом", 900m, 2800m), ("Пляшка для води", 150m, 600m),
            ("Ракетка", 800m, 4000m), ("Еспандер", 200m, 700m), ("Рюкзак", 700m, 2600m),
            ("Ролики", 2000m, 6000m)
        },
        ["Дім"] = new[]
        {
            ("Чайник", 600m, 2500m), ("Лампа", 400m, 1800m), ("Подушка", 300m, 1000m),
            ("Ковдра", 800m, 2800m), ("Сковорідка", 500m, 2000m), ("Чашка", 80m, 300m),
            ("Рушник", 150m, 500m), ("Годинник настінний", 350m, 1500m), ("Органайзер", 200m, 700m),
            ("Свічка", 80m, 350m)
        }
    };

    private static readonly string[] FirstNames =
    {
        "Денис", "Олена", "Максим", "Ірина", "Андрій", "Софія", "Богдан", "Марія", "Тарас", "Катерина",
        "Олег", "Анна", "Назар", "Юлія", "Віктор", "Дарина", "Роман", "Наталія", "Ігор", "Христина"
    };

    private static readonly string[] LastNames =
    {
        "Коваль", "Бондар", "Шевчук", "Мельник", "Ткачук", "Кравець", "Олійник", "Савчук",
        "Поліщук", "Лисенко", "Гончар", "Марчук", "Романюк", "Кушнір", "Левчук"
    };

    private static readonly string[] Cities =
    {
        "Рівне", "Київ", "Львів", "Луцьк", "Житомир", "Тернопіль", "Хмельницький", "Вінниця"
    };

    public static List<Product> CreateProducts(Random random)
    {
        List<Product> products = new List<Product>();
        int id = 1;

        foreach (KeyValuePair<string, (string Name, decimal MinPrice, decimal MaxPrice)[]> category in Catalog)
        {
            foreach ((string name, decimal minPrice, decimal maxPrice) in category.Value)
            {
                decimal price = minPrice + (decimal)random.NextDouble() * (maxPrice - minPrice);
                products.Add(new Product
                {
                    Id = id++,
                    Name = name,
                    Category = category.Key,
                    UnitPrice = Math.Round(price, 2)
                });
            }
        }

        return products; // 5 категорій × 10 товарів = 50
    }

    public static List<Customer> CreateCustomers(Random random, int count)
    {
        List<Customer> customers = new List<Customer>(count);

        for (int i = 1; i <= count; i++)
        {
            customers.Add(new Customer
            {
                Id = i,
                Name = $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}",
                City = Cities[random.Next(Cities.Length)]
            });
        }

        return customers;
    }

    public static List<Sale> CreateSales(Random random, int count, List<Product> products, List<Customer> customers)
    {
        List<Sale> sales = new List<Sale>(count);
        DateTime start = new DateTime(2025, 1, 1);
        int days = (new DateTime(2026, 10, 31) - start).Days + 1;

        for (int i = 1; i <= count; i++)
        {
            Product product = products[random.Next(products.Count)];
            // Дешеві товари купують більшими партіями
            int maxQuantity = product.UnitPrice < 1000 ? 6 : 3;

            sales.Add(new Sale
            {
                Id = i,
                ProductId = product.Id,
                CustomerId = customers[random.Next(customers.Count)].Id,
                Quantity = random.Next(1, maxQuantity + 1),
                SaleDate = start.AddDays(random.Next(days)).AddHours(random.Next(8, 22))
            });
        }

        return sales;
    }
}
