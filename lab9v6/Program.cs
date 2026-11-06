using System;
using System.Collections.Generic;

namespace lab15v6
{
    public class RestockOrder
    {
        public int OrderId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }

        public RestockOrder(int orderId, string productName, int quantity)
        {
            OrderId = orderId;
            ProductName = productName;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return $"Замовлення #{OrderId}: \"{ProductName}\" у кількості {Quantity} шт.";
        }
    }

    public class InventoryAction
    {
        public int ActionId { get; set; }
        public string ActionType { get; set; } // "Додавання" або "Видалення"
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }

        public InventoryAction(int actionId, string actionType, string productName, int quantity)
        {
            ActionId = actionId;
            ActionType = actionType;
            ProductName = productName;
            Quantity = quantity;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] [{ActionType}] {ProductName} ({Quantity} шт.)";
        }
    }
    public class InventoryManager
    {
        // 1. HashSet: зберігає тільки УНІКАЛЬНІ категорії товарів
        private readonly HashSet<string> _categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 2. Queue: черга замовлень на поповнення за принципом FIFO (First In, First Out)
        private readonly Queue<RestockOrder> _restockQueue = new Queue<RestockOrder>();

        // 3. Stack: історія виконаних операцій за принципом LIFO (Last In, First Out) для Undo
        private readonly Stack<InventoryAction> _actionHistory = new Stack<InventoryAction>();

        public bool AddCategory(string categoryName)
        {
            bool added = _categories.Add(categoryName);
            if (added)
            {
                Console.WriteLine($"  [HashSet] Категорію \"{categoryName}\" успішно додано.");
            }
            else
            {
                Console.WriteLine($"  [HashSet] ПОМИЛКА: Категорія \"{categoryName}\" вже існує! Дублікат ігнорується.");
            }
            return added;
        }

        public bool CheckCategoryExists(string categoryName)
        {
            bool exists = _categories.Contains(categoryName);
            Console.WriteLine($"  [HashSet] Перевірка наявності \"{categoryName}\": {(exists ? "Знайдено" : "Не знайдено")}");
            return exists;
        }

        public void DisplayCategories()
        {
            Console.WriteLine($"  [HashSet] Всього унікальних категорій: {_categories.Count}");
            foreach (var cat in _categories)
            {
                Console.WriteLine($"    • {cat}");
            }
        }
        public void EnqueueRestockOrder(RestockOrder order)
        {
            _restockQueue.Enqueue(order);
            Console.WriteLine($"  [Queue Enqueue] Додано в кінець черги: {order}");
        }

        public void ProcessNextRestockOrder()
        {
            if (_restockQueue.Count == 0)
            {
                Console.WriteLine("  [Queue] Черга замовлень порожня.");
                return;
            }

            // Peek — перегляд без видалення
            RestockOrder nextOrder = _restockQueue.Peek();
            Console.WriteLine($"  [Queue Peek] Наступним на обробку чекає: {nextOrder}");

            // Dequeue — видалення з початку черги (First In, First Out)
            RestockOrder processedOrder = _restockQueue.Dequeue();
            Console.WriteLine($"  [Queue Dequeue] ОБРОБЛЕНО ТА ВИДАЛЕНО З ЧЕРГИ: {processedOrder}");
        }

        public void DisplayQueue()
        {
            Console.WriteLine($"  [Queue] Замовлень у черзі на поповнення: {_restockQueue.Count}");
            foreach (var order in _restockQueue)
            {
                Console.WriteLine($"    • {order}");
            }
        }
        public void RecordAction(InventoryAction action)
        {
            _actionHistory.Push(action);
            Console.WriteLine($"  [Stack Push] Записано нову операцію в історію: {action}");
        }

        public void UndoLastAction()
        {
            if (_actionHistory.Count == 0)
            {
                Console.WriteLine("  [Stack] Історія операцій порожня. Немає що скасовувати.");
                return;
            }

            // Peek — перегляд вершини стек-історії
            InventoryAction topAction = _actionHistory.Peek();
            Console.WriteLine($"  [Stack Peek] На вершині стеку операція: {topAction}");

            // Pop — вилучення вершини (Last In, First Out)
            InventoryAction undoneAction = _actionHistory.Pop();
            Console.WriteLine($"  [Stack Pop] СКАСОВАНО (UNDO) ОПЕРАЦІЮ: {undoneAction}");
        }

        public void DisplayHistory()
        {
            Console.WriteLine($"  [Stack] Записів в історії: {_actionHistory.Count} (порядок від найновішого до найстарішого):");
            foreach (var action in _actionHistory)
            {
                Console.WriteLine($"    • {action}");
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №9 | ВАРІАНТ 6: Управління інвентарем");
            Console.WriteLine("    (HashSet<T> / Queue<T> / Stack<T>)");

            InventoryManager manager = new InventoryManager();

            Console.WriteLine("1. ПІДЗАДАЧА HASHSET<T>: Унікальні категорії товарів ");
            manager.AddCategory("Електроніка");
            manager.AddCategory("Побутова техніка");
            manager.AddCategory("Канцелярія");

            Console.WriteLine("\nСпроба додати дублікати категорій:");
            manager.AddCategory("Електроніка");  // Дублікат
            manager.AddCategory("канцелярія");   // Дублікат (з урахуванням ігнорування регістру)

            Console.WriteLine("\nПеревірка наявності елементів у HashSet (операція O(1)):");
            manager.CheckCategoryExists("Побутова техніка");
            manager.CheckCategoryExists("Одяг");

            Console.WriteLine("\nПоточний вміст HashSet:");
            manager.DisplayCategories();
            Console.WriteLine();

          
            Console.WriteLine(" 2. ПІДЗАДАЧА QUEUE<T>: Черга замовлень на поповнення (FIFO) ");
            manager.EnqueueRestockOrder(new RestockOrder(101, "Монітор 27\"", 5));
            manager.EnqueueRestockOrder(new RestockOrder(102, "Клавіатура USB", 15));
            manager.EnqueueRestockOrder(new RestockOrder(103, "Миша бездротова", 20));

            Console.WriteLine("\nПоточна черга перед обробкою:");
            manager.DisplayQueue();

            Console.WriteLine("\nПослідовна обробка замовлень з черги (Enqueue -> Peek -> Dequeue):");
            manager.ProcessNextRestockOrder(); // Обробить order 101 (найперший доданий)
            manager.ProcessNextRestockOrder(); // Обробить order 102

            Console.WriteLine("\nЗалишок у черзі після обробки двох замовлень:");
            manager.DisplayQueue();
            Console.WriteLine();

            Console.WriteLine("3. ПІДЗАДАЧА STACK<T>: Історія операцій та відміна (LIFO / Undo) ");
            manager.RecordAction(new InventoryAction(1, "Додавання", "Ноутбук Dell", 3));
            System.Threading.Thread.Sleep(200); // Невелика затримка для різниці в таймстампах
            manager.RecordAction(new InventoryAction(2, "Видалення", "Кабель HDMI", 10));
            System.Threading.Thread.Sleep(200);
            manager.RecordAction(new InventoryAction(3, "Додавання", "Принтер HP", 2));

            Console.WriteLine("\nПоточна історія операцій у стекі (зверху найновіша):");
            manager.DisplayHistory();

            Console.WriteLine("\nСкасування останніх дій (Push -> Peek -> Pop / Undo):");
            manager.UndoLastAction(); // Скасує "Принтер HP" (останній доданий)
            manager.UndoLastAction(); // Скасує "Кабель HDMI"

            Console.WriteLine("\nЗалишок історії в стекі після 2 скасувань:");
            manager.DisplayHistory();

           
            Console.WriteLine("ВИСНОВОК:");
            Console.WriteLine(" 1. HashSet<T> ефективно забезпечує унікальність елементів.");
            Console.WriteLine(" 2. Queue<T> гарантує прямий порядок обробки замовлень (FIFO).");
            Console.WriteLine(" 3. Stack<T> забезпечує зворотно-хронологічний порядок для Undo (LIFO).");
        }
    }
}