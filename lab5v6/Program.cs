using System;
using System.Collections.Generic;

namespace lab8v6
{

    // 1. БАЗОВИЙ КЛАС: Dish (Страва)
    public class Dish
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва страви не може бути порожньою.");
                _name = value;
            }
        }

        public Dish(string name)
        {
            Name = name;
        }

        // Віртуальний метод для перевизначення
        public virtual void Prepare()
        {
            Console.WriteLine($"[Dish] Починається загальне приготування страви: \"{Name}\"");
        }
    }


    // 2. ПОХІДНИЙ КЛАС 1: Pizza (Піца)
    public class Pizza : Dish
    {
        private string _toppings;

        public string Toppings
        {
            get => _toppings;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Перелік начинок не може бути порожнім.");
                _toppings = value;
            }
        }

        public Pizza(string name, string toppings) : base(name)
        {
            Toppings = toppings;
        }

        public override void Prepare()
        {
            Console.WriteLine($"[Pizza - OVERRIDE] Випікається піца \"{Name}\" з начинкою: {Toppings}.");
        }
    }

  
    // 3. ПОХІДНИЙ КЛАС 2: Soup (Суп)
    public class Soup : Dish
    {
        private string _brothType;

        public string BrothType
        {
            get => _brothType;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Тип бульйону не може бути порожнім.");
                _brothType = value;
            }
        }

        public Soup(string name, string brothType) : base(name)
        {
            BrothType = brothType;
        }

        public override void Prepare()
        {
            Console.WriteLine($"[Soup - OVERRIDE] Вариться суп \"{Name}\" на бульйоні: {BrothType}.");
        }
    }


    // 4. ПОХІДНИЙ КЛАС 3: Salad (Салат)
    public class Salad : Dish
    {
        private string _dressing;

        public string Dressing
        {
            get => _dressing;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Тип заправки не може бути порожнім.");
                _dressing = value;
            }
        }

        public Salad(string name, string dressing) : base(name)
        {
            Dressing = dressing;
        }

        public override void Prepare()
        {
            Console.WriteLine($"[Salad - OVERRIDE] Нарізається та заправляється салат \"{Name}\" (Заправка: {Dressing}).");
        }
    }


    // 5. ГОЛОВНИЙ КЛАС: Program (Точка входу)
    internal class Program
    {
        static void Main(string[] args)
        {
            // Налаштування кодування для коректного виводу українських літер у консолі
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №5 | ВАРІАНТ 6: Dish -> Pizza, Soup, Salad");
            

            // 1. Створення колекції об'єктів базового типу List<Dish>
            List<Dish> menu = new List<Dish>
            {
                new Pizza("Пепероні", "Сир моцарела, ковбаса пепероні, соус"),
                new Soup("Борщ український", "М'ясний яловичий"),
                new Salad("Цезар", "Соус Цезар, пармезан, сухарики"),
                new Pizza("Чотири сири", "Моцарела, дор блю, пармезан, чеддер"),
                new Soup("Грибний крем-суп", "Грибний відвар з вершками")
            };

            // Список для зберігання агрегованих результатів (приготованих страв)
            List<string> preparedDishesList = new List<string>();

            // 2. Демонстрація поліморфізму
            Console.WriteLine("--- 1. ПОЛІМОРФНИЙ ВИКЛИК МЕТОДУ Prepare() ДЛЯ ВСІХ СТРАВ ---");
            foreach (Dish dish in menu)
            {
                // Динамічне зв'язування: викликається перевизначений метод Prepare() відповідного похідного класу
                dish.Prepare();

                // Агрегація: зберігаємо результат роботи
                preparedDishesList.Add($"{dish.Name} [Тип: {dish.GetType().Name}]");
            }

            // 3. Виведення агрегованого результату
            Console.WriteLine("\n--- 2. АГРЕГАЦІЯ РЕЗУЛЬТАТІВ (ПІДСУМКОВИЙ СПИСОК ПРИГОТОВАНИХ СТРАВ) ---");
            Console.WriteLine($"Всього успішно приготовано страв: {preparedDishesList.Count}\n");
            Console.WriteLine("Перелік усіх приготованих страв:");
            for (int i = 0; i < preparedDishesList.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {preparedDishesList[i]}");
            }

            Console.WriteLine("ВИСНОВОК:");
            Console.WriteLine(" Використання поліморфізму дозволило обробити об'єкти різних класів");
            Console.WriteLine(" (Pizza, Soup, Salad) у єдиному циклі через посилання на базовий клас Dish.");

        }
    }
}