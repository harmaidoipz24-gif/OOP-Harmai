# Звіт до самостійної роботи №4

**Тема:** Розширення ієрархії та наслідки змін базового класу


## 1. Початкова ієрархія

```
LibraryItem (Title, PublicationYear, virtual DisplayDetails)
 ├── Book     (+ Author,      override DisplayDetails)
 └── Journal  (+ IssueNumber, override DisplayDetails)
```

Початковий конструктор базового класу:

```csharp
public LibraryItem(string title, int publicationYear) { ... }

public Book(string title, int publicationYear, string author)
    : base(title, publicationYear) { ... }

public Journal(string title, int publicationYear, int issueNumber)
    : base(title, publicationYear) { ... }
```

Ця версія компілювалася і працювала: `DisplayDetails()` для `Book` виводив базову інформацію + автора, для `Journal` — базову інформацію + номер випуску.

## 2. Модифікація базового класу (пункт 4)

До `LibraryItem` додано:

```csharp
private readonly string _uniqueId;
public string UniqueId => _uniqueId;

public LibraryItem(string uniqueId, string title, int publicationYear)
{
    _uniqueId = uniqueId;
    ...
}
```

### Які зміни довелося внести в похідні класи

Одразу після зміни конструктора проєкт **перестав компілюватися**: компілятор видав помилку CS7036 («There is no argument given that corresponds to the required parameter 'publicationYear'...») для обох похідних класів, бо виклики `base(title, publicationYear)` більше не відповідали жодному конструктору `LibraryItem`.

| Клас | Було | Стало |
|---|---|---|
| `Book` | `Book(string title, int publicationYear, string author) : base(title, publicationYear)` | `Book(string uniqueId, string title, int publicationYear, string author) : base(uniqueId, title, publicationYear)` |
| `Journal` | `Journal(string title, int publicationYear, int issueNumber) : base(title, publicationYear)` | `Journal(string uniqueId, string title, int publicationYear, int issueNumber) : base(uniqueId, title, publicationYear)` |
| `Main` | `new Book("Кобзар", 1840, "Тарас Шевченко")` | `new Book("BK-001", "Кобзар", 1840, "Тарас Шевченко")` |

Також оновлено `DisplayDetails()` у базовому класі, щоб виводити ідентифікатор. Похідні класи цю зміну отримали **автоматично**, бо викликають `base.DisplayDetails()`.

### Чому це сталося

1. **Конструктори не успадковуються.** Кожен похідний клас зобов'язаний викликати один із конструкторів базового класу (явно через `: base(...)` або неявно — конструктор без параметрів). У `LibraryItem` немає конструктора без параметрів, тому похідні класи жорстко залежать від його сигнатури.
2. **Зміна сигнатури — це зміна контракту.** Базовий клас став вимагати нові дані (`uniqueId`) для створення об'єкта, і цей обов'язок «протікає» вниз по всій ієрархії: кожен нащадок має прийняти значення і передати його далі.
3. **Ефект поширюється на всіх клієнтів.** Змінилися не лише `Book` і `Journal`, а й увесь код, що створює їхні об'єкти (`Main`). У великому проєкті таких місць можуть бути сотні — це проблема «крихкого базового класу» (fragile base class).

Натомість додавання **властивості** `UniqueId` і зміна тіла `DisplayDetails()` не вимагали змін у нащадках: вони отримали новий член автоматично. Отже, ламає ієрархію саме зміна *обов'язкового контракту* (конструктора), а не додавання нових можливостей.

> Як можна було уникнути каскаду змін: залишити старий конструктор поруч з новим (наприклад, `LibraryItem(string title, int year) : this(Guid.NewGuid().ToString("N")[..8], title, year)`) або генерувати ідентифікатор усередині базового класу. Тоді похідні класи компілювалися б без змін.

## 3. Розширення ієрархії новим класом AudioBook (пункт 5)

```csharp
public class AudioBook : Book
{
    private string _narrator;
    public string Narrator { get => _narrator; set => _narrator = value; }

    public AudioBook(string uniqueId, string title, int publicationYear, string author, string narrator)
        : base(uniqueId, title, publicationYear, author)
    {
        _narrator = narrator;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails();   // базова інформація + автор
        Console.WriteLine($"   Оповідач: {Narrator}");
    }
}
```

Додавання `AudioBook` **не вимагало жодних змін** в існуючих класах — це приклад принципу відкритості/закритості (OCP): ієрархію розширили новим класом, не модифікуючи старий код. Параметр `uniqueId` довелося додати до сигнатури з методички, бо після пункту 4 його вимагає весь ланцюжок конструкторів `AudioBook → Book → LibraryItem`.

Поліморфний виклик через `List<LibraryItem>` підтверджує правильність ієрархії: для кожного об'єкта викликається власна реалізація `DisplayDetails()`, а `AudioBook` через `base.DisplayDetails()` повторно використовує вивід і `LibraryItem`, і `Book`.

## 4. Переваги та недоліки наслідування

**Переваги:**
- Повторне використання коду: `Title`, `PublicationYear`, `UniqueId` і базовий вивід написані один раз.
- Поліморфізм: різні елементи бібліотеки зберігаються в одній колекції `List<LibraryItem>` і обробляються однаково.
- Легке розширення: новий тип (`AudioBook`) додається без змін існуючого коду.

**Недоліки:**
- Сильна зв'язність: зміна конструктора базового класу зламала компіляцію всіх нащадків і місць створення об'єктів.
- Глибокі ієрархії важко змінювати: `AudioBook` залежить одразу від `Book` і від `LibraryItem`; зміна будь-якого з них зачепить його.
- Наслідування жорстко фіксує структуру: якщо з'явиться, наприклад, «аудіожурнал», його не вийде красиво вбудувати — він мав би бути і `Journal`, і «аудіо», а в C# немає множинного наслідування класів.

## 5. Наслідування чи композиція?

**Наслідування доцільне**, коли виконується відношення «є» (*is-a*) і воно стабільне: книга *є* бібліотечним елементом, аудіокнига *є* книгою.

**Композиція краща**, коли відношення «має» (*has-a*) або поведінка комбінується. У цій задачі «аудіо» — це скоріше *формат*, а не новий вид книги. Альтернатива:

```csharp
public class AudioFormat
{
    public string Narrator { get; }
    public TimeSpan Duration { get; }
    public AudioFormat(string narrator, TimeSpan duration) { Narrator = narrator; Duration = duration; }
}

public class Book : LibraryItem
{
    public AudioFormat? Audio { get; }   // книга МАЄ (або не має) аудіоверсію
}
```

Тоді аудіоформат можна було б так само додати й до `Journal`, не створюючи `AudioBook`, `AudioJournal` тощо, а зміни в `AudioFormat` не зачіпали б ієрархію `LibraryItem`.

**Висновок:** наслідування добре працює для стабільних і неглибоких ієрархій, але робить базовий клас «крихким»: будь-яка зміна його обов'язкового контракту (конструктора, абстрактних методів) поширюється на всіх нащадків. Для змінних або комбінованих характеристик доцільніше використовувати композицію.

## Відповіді на контрольні запитання

1. **Як зміна конструктора базового класу вплинула на похідні класи?** — Похідні класи перестали компілюватися; довелося додати параметр `uniqueId` до їхніх конструкторів і передати його в `base(...)`, а також оновити всі виклики `new` у `Main`.
2. **Чим розширення через новий клас відрізняється від модифікації базового?** — Новий похідний клас (`AudioBook`) додає функціональність, не торкаючись існуючого коду (OCP). Модифікація базового класу змінює контракт для всіх нащадків одночасно і може зламати їх.
3. **Коли композиція могла б бути кращою?** — Для характеристик, які можуть комбінуватися з різними типами елементів (аудіоформат, електронна версія, стан примірника): їх зручніше винести в окремі класи-компоненти.
4. **Як поліморфний виклик `DisplayDetails()` допомагає перевірити ієрархію?** — Перебираючи `List<LibraryItem>`, ми бачимо, що для кожного об'єкта викликається правильний `override` і що ланцюжок `base.DisplayDetails()` виводить дані всіх рівнів ієрархії.
