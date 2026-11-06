# Звіт до самостійної роботи №14

**Тема:** Вивчення Dictionary<TKey, TValue> та HashSet<T> у бібліотеках .NET


## 1. Результати аналізу open-source проєктів

### Приклад 1. Dictionary у Dapper: таблиця відповідності типів

- **Проєкт:** Dapper (micro-ORM від Stack Overflow)
- **Файл:** [Dapper/SqlMapper.cs](https://github.com/StackExchange/Dapper/blob/684d7d9895e8b9eff4ab898528d69bd60979c098/Dapper/SqlMapper.cs)

```csharp
private static Dictionary<Type, DbType> typeMap;

static SqlMapper()
{
    typeMap = new Dictionary<Type, DbType>
    {
        [typeof(byte)] = DbType.Byte,
        [typeof(sbyte)] = DbType.SByte,
        [typeof(short)] = DbType.Int16,
        [typeof(ushort)] = DbType.UInt16,
        [typeof(int)] = DbType.Int32,
        [typeof(uint)] = DbType.UInt32,
        [typeof(long)] = DbType.Int64,
        [typeof(ulong)] = DbType.UInt64,
        // ...
```

Використання в методі `LookupDbType`:

```csharp
if (type.IsEnum() && !typeMap.ContainsKey(type))
{
    type = Enum.GetUnderlyingType(type);
}

if (typeMap.TryGetValue(type, out DbType dbType))
{
    return dbType;
}
```

- **Контекст:** коли Dapper передає параметр у SQL-запит, він має визначити тип стовпця бази даних (`DbType`) за типом C#-значення: `int` → `Int32`, `string` → `String` тощо. Це відбувається для **кожного параметра кожного запиту**.
- **Чому Dictionary:** потрібен пошук «за ключем» (типом). У `List<(Type, DbType)>` довелося б перебирати десятки пар для кожного параметра — O(n). Словник знаходить відповідність за O(1). Масив тут неможливий, бо ключ — `Type`, а не індекс.
- **Складність:** `TryGetValue` і `ContainsKey` — O(1) у середньому.
- **Особливості:** використано `TryGetValue` замість індексатора: один пошук замість двох (`ContainsKey` + `[]`) і жодного винятку для відсутнього типу. Comparer за замовчуванням: `Type` порівнюється за посиланням, і це правильно, бо кожен тип у процесі представлений одним об'єктом. Поруч у цьому ж файлі є `ConcurrentDictionary<Identity, CacheInfo> _queryCache` — кеш скомпільованих маперів, де потрібна потокобезпечність, бо запити виконуються з багатьох потоків одночасно.

### Приклад 2. HashSet у LINQ: реалізація Except

- **Проєкт:** .NET Runtime, бібліотека `System.Linq`
- **Файл:** [System/Linq/Except.cs](https://source.dot.net/System.Linq/System/Linq/Except.cs.html) (вихідний код `dotnet/runtime`)

```csharp
private static IEnumerable<TSource> ExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
{
    var set = new HashSet<TSource>(second, comparer);

    foreach (TSource element in first)
    {
        if (set.Add(element))
        {
            yield return element;
        }
    }
}
```

- **Контекст:** `first.Except(second)` повертає елементи першої послідовності, яких немає в другій, причому **без дублікатів** (це операція над множинами).
- **Чому HashSet:** потрібна лише перевірка «чи є такий елемент», значення до ключа не прив'язуються. Код використовує подвійну роль `Add`: він і перевіряє наявність, і додає елемент, повертаючи `false` для вже наявного. Тому один `HashSet` одночасно виключає елементи з `second` і прибирає дублікати з `first`. Зі `List.Contains` кожна перевірка була б O(m), а весь метод — O(n·m).
- **Складність:** побудова множини — O(m), прохід по `first` — O(n), разом O(n + m).
- **Особливості:** у конструктор передається `comparer`. Саме тому `Except` можна викликати, наприклад, з `StringComparer.OrdinalIgnoreCase` — логіка рівності повністю делегується `HashSet`. Метод «лінивий» (`yield return`): множина будується лише при першому перебиранні результату.

### Приклад 3 (власний код). Dictionary + HashSet разом

У власних класах `TagManager` і `SearchIndex` обидві колекції поєднуються: `Dictionary` швидко знаходить потрібну множину за ключем (статтею чи словом), а `HashSet` гарантує унікальність і дає операції над множинами.

## 2. Порівняльна таблиця

| Операція | List\<T\> | Dictionary<TKey,TValue> | HashSet\<T\> | Коли використовувати |
|---|---|---|---|---|
| Додавання | O(1)* в кінець, O(n) у середину | O(1)* | O(1)* | List — порядок важливий; Dictionary — пари ключ-значення; HashSet — лише унікальні значення |
| Пошук за значенням | O(n) | N/A (за значенням — O(n) через `ContainsValue`) | O(1) | HashSet, якщо часто перевіряємо «чи є елемент» |
| Пошук за ключем | N/A (за індексом — O(1)) | O(1) | N/A | Dictionary, коли за ідентифікатором треба отримати об'єкт |
| Видалення | O(n) (пошук + зсув елементів) | O(1) | O(1) | Dictionary/HashSet при частих видаленнях |
| Перевірка наявності | O(n) | O(1) (`ContainsKey`) | O(1) | Dictionary/HashSet для перевірок у циклах |

\* Амортизовано: зазвичай O(1), але коли внутрішній масив заповнюється, колекція виділяє новий, удвічі більший, і копіює всі елементи — O(n) для цієї однієї операції. Для Dictionary і HashSet O(1) — середній випадок; при поганій хеш-функції (багато колізій) операції деградують до O(n).

## 3. Висновки про вибір колекції

**Коли Dictionary краще за List?**
Коли об'єкти треба часто знаходити за ідентифікатором (ім'я файлу, id користувача, тип). Пошук у `List` через `Find`/`FirstOrDefault` — O(n), у словнику — O(1). Для 10 елементів різниця непомітна, але для 100 000 і пошуку в циклі — це секунди проти мілісекунд. List кращий, коли важливий порядок, потрібен доступ за індексом або дані просто перебираються від початку до кінця.

**Коли HashSet є оптимальним вибором?**
Коли значення не мають асоційованих даних і важливо лише «є чи немає»: унікальні теги, відвідані вузли графа, заборонені слова (стоп-слова), видалення дублікатів. А також для операцій над множинами — `UnionWith`, `IntersectWith`, `ExceptWith` реалізовані ефективно і роблять код коротким (див. `TagManager.FindCommonTags`).

**Підводні камені:**
- **Ключі та елементи мають бути незмінними.** Якщо змінити поле об'єкта, від якого залежить `GetHashCode`, після додавання в колекцію, об'єкт «загубиться» — пошук шукатиме його не в тому кошику.
- **Equals і GetHashCode мають бути узгоджені.** У `PersonNameComparer` обидва методи використовують лише `Name`; якщо хеш рахувати ще й з `Age`, «однакові» об'єкти потраплять у різні кошики і дублікат додасться.
- **Регістр рядків.** За замовчуванням `"CSharp"` і `"csharp"` — різні ключі. Потрібен `StringComparer.OrdinalIgnoreCase` (використано у `FileMetadataCache` і `TagManager`).
- **Індексатор кидає виняток.** `dict[key]` для відсутнього ключа кидає `KeyNotFoundException`; для «м'якого» пошуку краще `TryGetValue`.
- **Порядок не гарантований.** `Dictionary` і `HashSet` не обіцяють порядку перебирання, тому в демонстрації результати сортуються перед виводом.
- **Потокобезпечність.** Звичайні колекції не можна змінювати з кількох потоків — для цього є `ConcurrentDictionary` (як `_queryCache` у Dapper).
- **Пам'ять.** Хеш-колекції займають більше пам'яті, ніж `List`, через масив кошиків і збережені хеш-коди.

## 4. Приклади з власного коду

**FileMetadataCache** — пошук за ім'ям файлу без урахування регістру:

```csharp
private readonly Dictionary<string, FileMetadata> _cache =
    new Dictionary<string, FileMetadata>(StringComparer.OrdinalIgnoreCase);

public FileMetadata? Get(string fileName)
{
    return _cache.TryGetValue(fileName, out FileMetadata? metadata) ? metadata : null;
}
```

Вивід: `Get("REPORT.DOCX"): report.docx, 48213 байт, ...` — файл знайдено, хоча ім'я введено великими літерами.

**TagManager** — операції над множинами:

```csharp
public HashSet<string> FindCommonTags(string articleId1, string articleId2)
{
    HashSet<string> result = GetTags(articleId1);   // копія, щоб не зіпсувати оригінал
    result.IntersectWith(GetTags(articleId2));
    return result;
}
```

Вивід: `Спільні теги post-1 і post-2 (IntersectWith): {collections, csharp}`. Тег `CSharp` у статті `post-1` не додався повторно, бо `HashSet` з `OrdinalIgnoreCase` вважає його дублікатом `csharp`.

**SearchIndex** — інвертований індекс:

```csharp
public HashSet<int> SearchMultiple(params string[] words)
{
    HashSet<int> result = Search(words[0]);
    for (int i = 1; i < words.Length; i++)
        result.IntersectWith(Search(words[i]));
    return result;
}
```

Вивід: `SearchMultiple("c#", "java"): {3}` — лише третій документ містить обидва слова.

Повний вивід програми — у [README.md](README.md).

## Відповіді на контрольні запитання

1. **Переваги generic-колекцій над неузагальненими.** Типобезпечність (помилка типу видна під час компіляції, а не під час виконання), відсутність приведень типів і boxing/unboxing для значущих типів (у `ArrayList`/`Hashtable` кожен `int` упаковувався в об'єкт), а отже, вища швидкість і менше навантаження на пам'ять. Крім того, код читабельніший: `Dictionary<string, FileMetadata>` одразу пояснює, що зберігається.
2. **Dictionary чи HashSet.** `Dictionary` — коли за ключем треба отримати пов'язані дані (ім'я файлу → метадані, тип → `DbType`). `HashSet` — коли важлива лише наявність і унікальність значення або потрібні операції над множинами (теги, `Except` у LINQ).
3. **Роль IEqualityComparer\<T\>.** Він визначає, які ключі вважаються однаковими (`Equals`) і в який кошик потрапить елемент (`GetHashCode`). Від нього залежить коректність (з `OrdinalIgnoreCase` "CSharp" і "csharp" — один тег) і продуктивність: якщо хеш-функція повертає багато однакових значень, елементи збираються в одному кошику і пошук деградує з O(1) до O(n). Неузгоджені `Equals` і `GetHashCode` ламають колекцію.
4. **Чому `Dictionary<string, HashSet<int>>` для інвертованого індексу.** Словник за O(1) знаходить множину документів для слова, `HashSet<int>` автоматично не дає записати документ двічі, якщо слово в ньому повторюється, а пошук за кількома словами зводиться до `IntersectWith` (AND) або `UnionWith` (OR).
