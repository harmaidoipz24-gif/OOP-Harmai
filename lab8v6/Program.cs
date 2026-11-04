using System;
using System.Collections.Generic;

namespace lab14v6
{

    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

        public override string ToString()
        {
            return $"{Name} — {Price:C}";
        }
    }

       public static class CollectionHelper<T>
    {
              public static TAccumulate Aggregate<TAccumulate>(
            IEnumerable<T> collection,
            TAccumulate seed,
            Func<TAccumulate, T, TAccumulate> func)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection), "Колекція не може бути null.");

            if (func == null)
                throw new ArgumentNullException(nameof(func), "Функція-агрегатор не може бути null.");

            // Початковий стан акумулятора
            TAccumulate accumulator = seed;

            // Власна логіка перебору колекції в циклі foreach
            foreach (T item in collection)
            {
                accumulator = func(accumulator, item);
            }

            return accumulator;
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №8 | ВАРІАНТ 6: CollectionHelper<T>.Aggregate");

            Console.WriteLine(" 1. КОЛЕКЦІЯ ЧИСЕЛ (List<int>) ");
            List<int> numbers = new List<int> { 5, 10, 15, 20, 25 };
            Console.WriteLine($"Вхідні числа: {string.Join(", ", numbers)}");

            // Обчислення суми чисел за допомогою лямбда-виразу (acc, val) => acc + val
            int sum = CollectionHelper<int>.Aggregate(numbers, 0, (acc, num) => acc + num);
            Console.WriteLine($"1.1. Сума всіх чисел: {sum}");

            // Обчислення добутку чисел за допомогою лямбда-виразу (acc, val) => acc * val
            int product = CollectionHelper<int>.Aggregate(numbers, 1, (acc, num) => acc * num);
            Console.WriteLine($"1.2. Добуток усіх чисел: {product}\n");

           
            Console.WriteLine(" 2. КОЛЕКЦІЯ РЯДКІВ (List<string>)");
            List<string> words = new List<string> { "C#", "Делегати", "Лямбда-вирази", "Generics", "ООП" };
            Console.WriteLine($"Вхідні слова: {string.Join(" | ", words)}");

            // Склеювання рядків у єдину фразу через дефіс
            string concatenated = CollectionHelper<string>.Aggregate(
                words,
                "ТЕГИ:",
                (acc, word) => $"{acc} [{word}]"
            );
            Console.WriteLine($"2.1. Результат конкатенації: {concatenated}\n");

           
            Console.WriteLine(" 3. КОЛЕКЦІЯ ОБ'ЄКТІВ КЛАСУ (List<Product>)");
            List<Product> cart = new List<Product>
            {
                new Product("Ноутбук", 28500.00m),
                new Product("Бездротова миша", 650.50m),
                new Product("Механічна клавіатура", 2400.00m),
                new Product("Монітор 27\"", 9200.00m)
            };

            Console.WriteLine("Список товарів у кошику:");
            foreach (var prod in cart)
            {
                Console.WriteLine($"  • {prod}");
            }

            // Обчислення загальної вартості всіх товарів
            decimal totalPrice = CollectionHelper<Product>.Aggregate(
                cart,
                0.00m,
                (acc, productItem) => acc + productItem.Price
            );

            Console.WriteLine($"\n3.1. Загальна вартість усіх товарів: {totalPrice:C}");

            Console.WriteLine("ВИСНОВОК:");
            Console.WriteLine(" Метод CollectionHelper<T>.Aggregate успішно агрегує дані різних");
            Console.WriteLine(" типів (int, string, Product), приймаючи логіку через лямбда-вирази.");
        }
    }
}