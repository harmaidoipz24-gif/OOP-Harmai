# Звіт до самостійної роботи №5

**Тема:** Приклади поліморфізму в .NET
**Виконав:** Гармай Денис

У роботі досліджено три механізми поліморфізму в стандартній бібліотеці .NET: віртуальні методи базового класу `System.Object`, інтерфейс `IEnumerable<T>` і абстрактний клас `System.IO.Stream`. У кожному випадку **один клієнтський код** (`Console.WriteLine`, `foreach`, `ProcessStream`) працює з **різними реалізаціями**.

## 1. Поліморфізм через `System.Object`

Будь-який клас у C# неявно успадковується від `System.Object` і отримує його віртуальні методи `ToString()`, `Equals()`, `GetHashCode()`. Класи `Product`, `Customer` і `Order` перевизначають `ToString()`:

```csharp
public class Product
{
    public string Name { get; }
    public decimal Price { get; }
    public override string ToString() => $"Product: {Name}, ціна {Price:F2} грн";
}

public class Customer
{
    public override string ToString() => $"Customer: {FullName} <{Email}>";
}
```

Об'єкти зберігаються в колекції з елементами типу `object`:

```csharp
List<object> objects = new List<object> { laptop, customer, order, new Warehouse(), 42, date };

foreach (object obj in objects)
    Console.WriteLine($"{obj.GetType().Name,-10} -> {obj}");
```

Результат:

```
Product    -> Product: Laptop, ціна 35000,00 грн
Customer   -> Customer: Денис Гармай <denys@example.com>
Order      -> Order #1001: Денис Гармай, товарів 2, сума 35800,00 грн
Warehouse  -> IndependentWork5.Warehouse
Int32      -> 42
DateOnly   -> 22.10.2026
```

**Як проявляється поліморфізм.** Статичний тип змінної `obj` — `object`, але `ToString()` віртуальний, тому під час виконання CLR через таблицю віртуальних методів викликає реалізацію *фактичного* типу об'єкта (пізнє зв'язування). Клас `Warehouse` метод не перевизначив, тому спрацювала базова версія `Object.ToString()`, яка повертає лише назву типу. Навіть вбудовані `int` і `DateOnly` мають власні перевизначення.

**Переваги.** `Console.WriteLine`, інтерполяція рядків, логери і налагоджувач написані один раз для типу `object`, але коректно показують будь-який наш клас — достатньо перевизначити один метод. Немає потреби в `if (obj is Product) ... else if (obj is Customer) ...`.

## 2. Поліморфізм через інтерфейс `IEnumerable<T>`

Цикл `foreach` працює з будь-яким типом, що надає метод `GetEnumerator()`. Власна колекція реалізує `IEnumerable<string>`:

```csharp
public class MyCustomCollection : IEnumerable<string>
{
    private readonly List<string> _items = new List<string>();

    public void Add(string item) => _items.Add(item);

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string item in _items)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

`IEnumerable<T>` успадковує неузагальнений `IEnumerable`, тому треба реалізувати обидва методи `GetEnumerator()`; неузагальнений реалізовано явно і просто делегує узагальненому. Ключове слово `yield return` змушує компілятор самостійно згенерувати клас-перелічувач із `MoveNext()` і `Current`.

Один метод приймає будь-яку колекцію рядків:

```csharp
private static void PrintAll(string label, IEnumerable<string> items)
    => Console.WriteLine($"{label}: {string.Join(", ", items)}");

PrintAll("MyCustomCollection", courses);
PrintAll("string[]", array);
PrintAll("List<string>", list);
```

**Як проявляється поліморфізм.** `foreach`, `string.Join` і `PrintAll` залежать лише від інтерфейсу. Масив, `List<string>` і `MyCustomCollection` мають зовсім різну внутрішню будову, але всі «вміють» видавати перелічувач, тож перебираються однаково. Це поліморфізм через інтерфейс (контракт), без спільного базового класу.

**Переваги.** Реалізувавши лише `GetEnumerator()`, наша колекція автоматично отримала підтримку `foreach`, `string.Join`, конструкторів інших колекцій і всього LINQ (`courses.Count(c => c.Length > 5)` у програмі). Внутрішній `List<string>` залишився прихованим — ззовні колекцію можна лише перебирати, але не змінити напряму (інкапсуляція).

## 3. Поліморфізм через абстрактний клас `Stream`

`System.IO.Stream` — абстрактний клас із абстрактними членами (`Read`, `Write`, `Seek`, `Length`, `CanRead`…). `FileStream`, `MemoryStream`, `NetworkStream` та інші реалізують їх по-своєму.

```csharp
private static void ProcessStream(Stream stream)
{
    Console.WriteLine($"Тип потоку: {stream.GetType().Name}");
    Console.WriteLine($"  CanRead: {stream.CanRead}, CanSeek: {stream.CanSeek}, довжина: {stream.Length} байт");

    byte[] buffer = new byte[10];
    int bytesRead = stream.Read(buffer, 0, buffer.Length);
    Console.WriteLine($"  Як текст: \"{Encoding.UTF8.GetString(buffer, 0, bytesRead)}\"");
}
```

Тестовий файл створюється в коді (`File.WriteAllText`), тому проєкт самодостатній:

```csharp
File.WriteAllText(filePath, "Hello from FileStream!", Encoding.UTF8);
using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
    ProcessStream(fileStream);

using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello from MemoryStream!")))
    ProcessStream(memoryStream);
```

**Як проявляється поліморфізм.** Параметр має абстрактний тип `Stream`, створити його екземпляр неможливо — передається лише конкретний нащадок. Виклик `stream.Read(...)` для `FileStream` звертається до операційної системи й читає байти з диска, а для `MemoryStream` — копіює їх із масиву в пам'яті. Код `ProcessStream` при цьому однаковий і нічого не знає про джерело даних.

**Переваги.** Метод можна без змін використати для файлу, пам'яті, мережі, стиснутих (`GZipStream`) чи зашифрованих (`CryptoStream`) даних. Для тестування замість реального файлу зручно передавати `MemoryStream`. Абстрактний клас, на відміну від інтерфейсу, ще й надає готову спільну логіку (наприклад, `CopyTo`, `ReadAsync`), яку нащадки отримують даром.

## 4. Загальні висновки

| Сценарій | Механізм | Спільний клієнтський код | Що змінюється |
|---|---|---|---|
| `Object` | віртуальний метод + `override` | `Console.WriteLine(obj)` | текст, який повертає `ToString()` |
| `IEnumerable<T>` | реалізація інтерфейсу | `foreach`, LINQ, `PrintAll` | спосіб зберігання і перебору елементів |
| `Stream` | абстрактний клас | `ProcessStream(Stream)` | джерело даних (диск / пам'ять) |

Поліморфізм дозволяє писати код відносно **абстракції**, а не конкретного типу. Це дає гнучкість (нові типи підключаються без зміни існуючого коду), повторне використання (один метод замість кількох майже однакових) і простоту тестування (реальну залежність легко підмінити, як `FileStream` на `MemoryStream`).

## Відповіді на контрольні запитання

1. **Як поліморфізм проявляється при роботі з `List<object>`?** — Елементи мають статичний тип `object`, але при виклику віртуального `ToString()` виконується перевизначена реалізація фактичного класу кожного елемента.
2. **Що потрібно реалізувати в `MyCustomCollection` для `foreach`?** — Інтерфейс `IEnumerable<string>`, тобто метод `IEnumerator<string> GetEnumerator()` і (явно) неузагальнений `IEnumerator IEnumerable.GetEnumerator()`.
3. **Чому `ProcessStream(Stream stream)` — приклад поліморфної роботи з абстрактним класом?** — Метод приймає абстрактний тип, а викликані в ньому `Read`, `Length`, `CanSeek` визначаються в конкретних нащадках; той самий код працює з `FileStream` і `MemoryStream`.
4. **Які переваги дає єдиний інтерфейс для різних потоків і колекцій?** — Код пишеться один раз і працює з будь-якою існуючою чи майбутньою реалізацією; реалізації легко замінювати (зокрема в тестах); клієнтський код не залежить від деталей зберігання даних.
