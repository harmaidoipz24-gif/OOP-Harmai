public class BankAccount
{
    private string owner;
    private string accountNumber;
    private decimal balance;

    public decimal Balance => balance;
    public string Owner => owner;
    public string AccountNumber => accountNumber;

    public BankAccount(string owner, string accountNumber, decimal initialBalance)
    {
        this.owner = owner;
        this.accountNumber = accountNumber;
        this.balance = initialBalance;
    }

    public void Deposit(decimal amount)
    {
        if (amount > 0)
        {
            balance += amount;
            Console.WriteLine("[+] Поповнено на " + amount + " грн. Баланс: " + balance + " грн.");
        }
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= balance)
        {
            balance -= amount;
            Console.WriteLine("[-] Знято " + amount + " грн. Баланс: " + balance + " грн.");
        }
        else
        {
            Console.WriteLine("[-] Недостатньо коштів на рахунку!");
        }
    }
}