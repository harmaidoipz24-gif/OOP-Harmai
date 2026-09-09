using System;

namespace Lab2App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Початок виконання програми ===");

            // Викликаємо локальний метод, щоб після його завершення об'єкти втратили посилання
            CreateAccounts();

            Console.WriteLine("\n=== КінецьMain, підготовка до очищення пам'яті ===");
            
            // Примусовий виклик Garbage Collector (лише для навчальних цілей!)
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("=== Програму повністю завершено ===");
        }

        static void CreateAccounts()
        {
            Console.WriteLine("\n--- 1. Створення об'єктів ---");

            // Виклик повного конструктора
            BankAccount acc1 = new BankAccount("Іван Петренко", "UA1001", 1500m);

            // Виклик конструктора з 2 параметрами (застосовує : this)
            BankAccount acc2 = new BankAccount("Олена Коваль", "UA1002");

            Console.WriteLine("\n--- 2. Виконання операцій з рахунками ---");

            // Операції з першим рахунком
            acc1.Deposit(500m);
            acc1.Withdraw(300m);
            acc1.Withdraw(5000m); // Демонстрація валідації (недостатньо коштів)

            // Операції з другим рахунком
            acc2.Deposit(2000m);
            acc2.Withdraw(450m);

            Console.WriteLine("\n--- 3. Підсумковий стан ---");
            Console.WriteLine($"Рахунок {acc1.AccountNumber} ({acc1.Owner}): {acc1.Balance} UAH");
            Console.WriteLine($"Рахунок {acc2.AccountNumber} ({acc2.Owner}): {acc2.Balance} UAH");
        }
    }
}