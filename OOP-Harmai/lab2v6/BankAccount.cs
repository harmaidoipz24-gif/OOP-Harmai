using System;

namespace Lab2App
{
    public class BankAccount
    {
        // Приватні поля
        private string _owner;
        private string _accountNumber;
        private decimal _balance;

        // Властивості
        public string Owner
        {
            get => _owner;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ім'я власника не може бути порожнім.");
                _owner = value;
            }
        }

        // Властивості лише для читання
        public string AccountNumber => _accountNumber;
        public decimal Balance => _balance;

        // 1. Повний конструктор із параметрами
        public BankAccount(string owner, string accountNumber, decimal initialBalance)
        {
            if (initialBalance < 0)
                throw new ArgumentException("Початковий баланс не може бути від'ємним.");

            _owner = owner;
            _accountNumber = accountNumber;
            _balance = initialBalance;
            
            Console.WriteLine($"[Конструктор] Створено рахунок {accountNumber} для {owner} з балансом {initialBalance} UAH.");
        }

        // 2. Перевантажений конструктор (викликає перший через : this)
        public BankAccount(string owner, string accountNumber) 
            : this(owner, accountNumber, 0m)
        {
            Console.WriteLine($"[Конструктор] Викликано перевантажений конструктор (баланс за замовчуванням 0 UAH).");
        }

        // Метод поповнення
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Помилка: Сума поповнення має бути більшою за 0.");
                return;
            }

            _balance += amount;
            Console.WriteLine($"[Deposit] {Owner} поповнив(-ла) рахунок на {amount} UAH. Баланс: {_balance} UAH.");
        }

        // Метод зняття коштів з валідацією
        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Помилка: Сума зняття має бути більшою за 0.");
                return false;
            }

            if (amount > _balance)
            {
                Console.WriteLine($"Помилка: Недостатньо коштів на рахунку {AccountNumber}. Спроба зняти {amount} UAH при балансі {_balance} UAH.");
                return false;
            }

            _balance -= amount;
            Console.WriteLine($"[Withdraw] З рахунку {Owner} знято {amount} UAH. Залишок: {_balance} UAH.");
            return true;
        }

        // Деструктор (Фіналізатор)
        ~BankAccount()
        {
            Console.WriteLine($"[Деструктор] Об'єкт рахунку {AccountNumber} ({Owner}) знищено збирачем сміття.");
        }
    }
}