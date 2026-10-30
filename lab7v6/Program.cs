using System;
using System.Collections.Generic;
using System.Linq;

namespace lab7v6
{

    public interface IEntity
    {
        int Id { get; set; }
    }

    public class Employee : IEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        public Employee() { }

        public Employee(int id, string firstName, string lastName, string position)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
        }

        public override string ToString()
        {
            return $"[ID: {Id}] {FirstName} {LastName} — Посада: {Position}";
        }
    }


    public interface IRepository<T> where T : IEntity
    {
        T? GetById(int id);
        IEnumerable<T> GetAll();
        void Add(T item);
        void Update(T item);
        void Delete(int id);
    }


    public class InMemoryRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly List<T> _items = new List<T>();

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Елемент не може бути null.");

            if (_items.Any(x => x.Id == item.Id))
                throw new InvalidOperationException($"Елемент з ID {item.Id} вже існує.");

            _items.Add(item);
        }

        public T? GetById(int id)
        {
            return _items.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<T> GetAll()
        {
            return _items.ToList();
        }

        public void Update(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Елемент не може бути null.");

            int index = _items.FindIndex(x => x.Id == item.Id);
            if (index == -1)
                throw new KeyNotFoundException($"Елемент з ID {item.Id} не знайдено для оновлення.");

            _items[index] = item;
        }

        public void Delete(int id)
        {
            T? existing = GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Елемент з ID {id} не знайдено для видалення.");

            _items.Remove(existing);
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №7 | ВАРІАНТ 6: Generic IRepository<Employee>");
            
            IRepository<Employee> repository = new InMemoryRepository<Employee>();

            // 1. Додавання
            Console.WriteLine(" 1. ДОДАВАННЯ ПРАЦІВНИКІВ (Add) ");
            repository.Add(new Employee(1, "Олександр", "Шевченко", "Middle C# Developer"));
            repository.Add(new Employee(2, "Марія", "Коваленко", "QA Engineer"));
            repository.Add(new Employee(3, "Іван", "Петренко", "Project Manager"));

            foreach (var emp in repository.GetAll())
            {
                Console.WriteLine(emp);
            }
            Console.WriteLine();

            // 2. Отримання за ID
            Console.WriteLine(" 2. ОТИМАННЯ ЗА ID (GetById) ");
            var foundEmp = repository.GetById(2);
            Console.WriteLine($"Знайдено елемент з ID 2: {foundEmp}\n");

            // 3. Оновлення
            Console.WriteLine(" 3. ОНОВЛЕННЯ ЕЛЕМЕНТА (Update) ");
            if (foundEmp != null)
            {
                foundEmp.Position = "Senior QA Engineer";
                repository.Update(foundEmp);
                Console.WriteLine($"Оновлено посаду для ID 2: {repository.GetById(2)}\n");
            }

            // 4. Видалення
            Console.WriteLine(" 4. ВИДАЛЕННЯ ЕЛЕМЕНТА (Delete)");
            repository.Delete(1);
            Console.WriteLine("Видалено працівника з ID 1. Залишок у репозиторії:");
            foreach (var emp in repository.GetAll())
            {
                Console.WriteLine(emp);
            }

            Console.WriteLine("  Основна перевірка здійснюється через юніт-тести xUnit!");
            
        }
    }
}