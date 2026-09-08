

BankAccount acc1 = new BankAccount("Іван Петренко", "UA1001", 1000.00m);
BankAccount acc2 = new BankAccount("Олена Коваль", "UA1002", 5000.50m);
BankAccount acc3 = new BankAccount("Максим Сидоренко", "UA1003", 250.00m);

Console.WriteLine("=== ДЕМОНСТРАЦІЯ РОБОТИ БАНКІВСЬКИХ РАХУНКІВ ===");
Console.WriteLine();

acc1.Deposit(500.00m);
acc1.Withdraw(200.00m);

Console.WriteLine();

acc2.Withdraw(6000.00m);
acc2.Withdraw(1500.00m);

Console.WriteLine();

acc3.Deposit(1000.00m);
Console.WriteLine("Підсумковий баланс: " + acc3.Balance + " грн.");