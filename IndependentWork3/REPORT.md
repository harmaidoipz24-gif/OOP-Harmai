# Звіт з аналізу інкапсуляції в Open-Source проєкті

## 1. Обраний проєкт

- **Назва:** .NET Runtime (`dotnet/runtime`) — середовище виконання і базова бібліотека класів .NET (CoreCLR, `System.Private.CoreLib`, `System.*`)
- **Посилання на GitHub:** https://github.com/dotnet/runtime
- **Ліцензія:** MIT
- **Проаналізовані файли:**
  - `List<T>` — [src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs)
  - `Version` — [src/libraries/System.Private.CoreLib/src/System/Version.cs](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Version.cs)

Проєкт обрано тому, що його класи використовує кожен C#-розробник, а код написаний з високими вимогами до надійності й продуктивності — тому добре видно, як інкапсуляція поєднується з оптимізацією.

## 2. Аналіз інкапсуляції

### Клас: `List<T>`

- **Опис класу:** узагальнений динамічний масив — найпоширеніша колекція в .NET. Зберігає елементи у внутрішньому масиві й автоматично збільшує його при додаванні.

- **Поля:** усі поля стану закриті від зовнішнього коду. Вони позначені `internal` (а не `private`) — доступні лише всередині збірки `System.Private.CoreLib`; коментарі пояснюють, що від точних назв залежить бінарна серіалізація та внутрішні механізми runtime. Порожній масив-заглушка — `private static readonly`, спільний для всіх списків.

  ```csharp
  private const int DefaultCapacity = 4;

  internal T[] _items; // Do not rename (binary serialization) ...
  internal int _size;  // Do not rename (binary serialization) ...
  internal int _version; // Do not rename (binary serialization)

  private static readonly T[] s_emptyArray = new T[0];
  ```

  Ззовні неможливо отримати сам масив `_items` — інакше користувач міг би змінити елементи за межами `_size` або «зламати» список. Поле `_version` збільшується при кожній зміні; перелічувач порівнює його, щоб виявити модифікацію колекції під час `foreach` — це внутрішній інваріант, який теж має бути прихованим.

- **Властивості:**

  ```csharp
  // Read-only property describing how many elements are in the List.
  public int Count => _size;

  public int Capacity
  {
      get => _items.Length;
      set { /* перевірка і перевиділення масиву, див. розділ 3 */ }
  }
  ```

  - `Count` — **тільки для читання** (expression-bodied `=>`, без `set`). Кількість елементів змінюється лише через методи `Add`, `Remove`, `Clear`, які одночасно підтримують `_items` і `_version` у узгодженому стані.
  - `Capacity` — **читання й запис**, але `set` — це не просте присвоєння, а логіка: перевірка і створення нового масиву з копіюванням елементів. Властивість приховує деталь реалізації (масив) і дає контрольований доступ до неї.
  - **Автоматичних властивостей** у класі немає: стан складний і взаємопов'язаний, тож розробники явно керують полями.

- **Індексатори/Оператори:** клас має індексатор `this[int index]` — завдяки йому список використовується як масив (`list[0] = x`), але кожен доступ проходить перевірку меж (код у розділі 3). Перевантажених операторів немає.

### Клас: `Version`

- **Опис класу:** представляє номер версії у форматі `major.minor[.build[.revision]]` (наприклад, `8.0.1`). Незмінний (immutable) клас.

- **Поля:** чотири **`private readonly`** поля — їх неможливо змінити навіть усередині класу після завершення конструктора.

  ```csharp
  private readonly int _Major;    // Do not rename (binary serialization)
  private readonly int _Minor;    // Do not rename (binary serialization)
  private readonly int _Build;    // Do not rename (binary serialization)
  private readonly int _Revision; // Do not rename (binary serialization)
  ```

- **Властивості:** лише **read-only**, без `set` взагалі:

  ```csharp
  public int Major => _Major;
  public int Minor => _Minor;
  public int Build => _Build;
  public int Revision => _Revision;
  ```

  Це повна протилежність `List<T>`: стан задається один раз у конструкторі й більше не змінюється. Через це екземпляр `Version` безпечно передавати будь-куди, зберігати в словниках як ключ і використовувати з кількох потоків.

- **Індексатори/Оператори:** індексатора немає, зате перевантажено оператори порівняння:

  ```csharp
  public static bool operator ==(Version? v1, Version? v2)
  public static bool operator <(Version? v1, Version? v2)
  // а також !=, >, <=, >=
  ```

  Оператори дають природний синтаксис `if (current < required)` і порівнюють версії **за змістом** (по компонентах), а не за посиланням. Сигнатури приймають `Version?`, тобто коректно обробляють `null`. Інкапсуляція тут проявляється в тому, що правила порівняння (наприклад, як порівнювати `1.2` і `1.2.0`) сховані всередині класу, а не розкидані по коду користувачів.

## 3. Практики валідації у властивостях

- **Приклад 1: валідація в `set` аксесорі — `List<T>.Capacity`**

  ```csharp
  public int Capacity
  {
      get => _items.Length;
      set
      {
          if (value < _size)
          {
              ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
          }

          if (value != _items.Length)
          {
              if (value > 0)
              {
                  T[] newItems = new T[value];
                  if (_size > 0)
                  {
                      Array.Copy(_items, newItems, _size);
                  }
                  _items = newItems;
              }
              else
              {
                  _items = s_emptyArray;
              }
          }
      }
  }
  ```

  Сценарій: користувач намагається зменшити місткість нижче за кількість елементів, що призвело б до втрати даних. `if`-перевірка відкидає таке значення і кидає `ArgumentOutOfRangeException`. Друга перевірка (`value != _items.Length`) — оптимізація: якщо місткість не змінюється, зайве перевиділення пам'яті не відбувається.

- **Приклад 2: валідація в індексаторі — `List<T>.this[int]`**

  ```csharp
  public T this[int index]
  {
      get
      {
          // Following trick can reduce the range check by one
          if ((uint)index >= (uint)_size)
          {
              ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
          }
          return _items[index];
      }
      set
      {
          if ((uint)index >= (uint)_size)
          {
              ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
          }
          _items[index] = value;
          _version++;
      }
  }
  ```

  Важливо: внутрішній масив може бути довшим за список (`Capacity > Count`), тож вбудована перевірка меж масиву не допомогла б — без цієї перевірки `list[Count]` повернув би «сміття» з резерву. Цікавий прийом: приведення до `uint` перетворює від'ємний індекс на дуже велике число, тому **одна** перевірка замінює дві (`index < 0 || index >= _size`). У `set` після запису збільшується `_version`, щоб перелічувачі виявили зміну.

- **Приклад 3: валідація в конструкторі — `Version(int, int, int)` і `List<T>(int)`**

  ```csharp
  public Version(int major, int minor, int build)
  {
      ArgumentOutOfRangeException.ThrowIfNegative(major);
      ArgumentOutOfRangeException.ThrowIfNegative(minor);
      ArgumentOutOfRangeException.ThrowIfNegative(build);

      _Major = major;
      _Minor = minor;
      _Build = build;
      _Revision = -1;
  }
  ```

  ```csharp
  public List(int capacity)
  {
      if (capacity < 0)
          ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

      if (capacity == 0)
          _items = s_emptyArray;
      else
          _items = new T[capacity];
  }
  ```

  У незмінному класі `Version` немає `set`, тому **вся валідація зосереджена в конструкторі**: якщо об'єкт створено, він гарантовано коректний. Використано сучасні guard-методи .NET 8 `ArgumentOutOfRangeException.ThrowIfNegative(...)` — вони автоматично підставляють назву параметра (через `CallerArgumentExpression`) і замінюють трирядкові `if`-блоки одним рядком. Значення `_Revision = -1` позначає «компонент не задано».

- **Висновки щодо валідації:**
  - Використовуються **імперативні перевірки** (`if` + виняток) і guard-методи `ThrowIfNegative`. Атрибутів `System.ComponentModel.DataAnnotations` у цих класах немає — і це доречно: атрибути перевіряються лише тоді, коли їх явно запускає валідатор (наприклад, ASP.NET Core при прив'язці моделі), тоді як базова бібліотека має гарантувати коректність *завжди*.
  - Виняток `ArgumentOutOfRangeException` обрано правильно: він точно вказує тип помилки (значення поза допустимим діапазоном).
  - Винятки кидаються через `ThrowHelper` — це прийом продуктивності: метод, який лише кидає виняток, JIT не вбудовує в «гарячий» код, тому властивості й індексатор залишаються маленькими та швидкими. Недолік — читати такий код важче, ніж звичайний `throw new ...`.
  - Перевірка в `set` дозволяє зберегти інваріанти (`Capacity >= Count`, `0 <= index < Count`), а відсутність `set` (у `Version`) усуває саму можливість зіпсувати стан після створення.

## 4. Загальні висновки

1. Обидва класи суворо дотримуються інкапсуляції: **жодне поле стану не є публічним**, доступ — тільки через властивості, індексатор і методи.
2. Показано два різні підходи. `List<T>` — **змінний** об'єкт: має `set`-аксесори, але кожен із них перевіряє дані й підтримує інваріанти. `Version` — **незмінний** об'єкт: `readonly` поля, тільки `get`, уся валідація — в конструкторі. Другий підхід простіший і безпечніший, перший потрібен там, де об'єкт за своєю природою змінюється.
3. Властивості — це не «обгортки над полями». `Count` обчислюється з `_size`, `Capacity` приховує масив і перевиділяє пам'ять, тож внутрішню реалізацію можна змінювати, не ламаючи коду користувачів.
4. Цікаво, що поля `List<T>` мають модифікатор `internal`, а не `private`: інкапсуляція тут проведена на рівні **збірки** — це компроміс заради внутрішніх оптимізацій runtime, явно задокументований у коментарях.
5. Порівняно з навчальними прикладами, у промисловому коді валідація поєднана з оптимізацією (`(uint)` трюк, `ThrowHelper`, повторне використання `s_emptyArray`). Для власних проєктів варто перейняти головне: перевіряти дані на межі класу і робити об'єкти незмінними там, де це можливо.

## Відповіді на контрольні запитання

1. **За якими ознаками можна зробити висновок, що в класі дотримано інкапсуляції?** — Поля приватні (або щонайменше не публічні); доступ лише через властивості й методи; `set` відсутній або містить перевірку; внутрішні структури (масив `_items`) не повертаються назовні; інваріанти не можна порушити ззовні.
2. **Які підходи до валідації ви зустріли?** — `if`-перевірки в `set` і індексаторі з викиданням `ArgumentOutOfRangeException` через `ThrowHelper`; guard-методи `ArgumentOutOfRangeException.ThrowIfNegative` у конструкторі; незмінність (`readonly`) як спосіб узагалі уникнути валідації після створення.
3. **Чому важливо наводити посилання на конкретні класи та фрагменти коду?** — Щоб висновки можна було перевірити; код великих проєктів змінюється, тож посилання фіксує, що саме аналізувалося; без коду аналіз стає загальними словами.
4. **Які висновки щодо якості проєктування після порівняння класів?** — Вибір між змінним і незмінним класом визначає, де розміщується валідація; незмінний `Version` простіший для розуміння і потокобезпечний, а `List<T>` показує, як зберегти коректність змінного об'єкта, контролюючи кожну точку зміни стану.
