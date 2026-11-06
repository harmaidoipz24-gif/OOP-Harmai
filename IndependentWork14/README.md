# IndependentWork14 — Dictionary і HashSet

Самостійна робота №14.

Консольний застосунок демонструє три сценарії:

1. `FileMetadataCache` — кеш метаданих файлів на `Dictionary<string, FileMetadata>` з `StringComparer.OrdinalIgnoreCase`.
2. `TagManager` — теги статей на `Dictionary<string, HashSet<string>>` з операціями `UnionWith`, `IntersectWith`, `ExceptWith`.
3. `SearchIndex` (додатково) — інвертований індекс `Dictionary<string, HashSet<int>>`.

Також показано custom comparer для `Dictionary` і `HashSet<Person>`.

Звіт з аналізом open-source прикладів і таблицею складності — [REPORT.md](REPORT.md).

## Приклад виводу

```
Сценарій 1. FileMetadataCache (Dictionary)
У кеші файлів: 3
Get("REPORT.DOCX"): report.docx, 48213 байт, змінено 02.11.2026 14:30, hash a1f3c9
Contains("photo.png"): True
Remove("photo.png"): True
Contains("photo.png") після видалення: False
Get("missing.txt") повертає null: True

Сценарій 2. TagManager (HashSet)
   Тег "CSharp" уже є у статті post-1 — дублікат не додано
Усі унікальні теги (UnionWith): {beginners, collections, csharp, dotnet, linq, python}
Спільні теги post-1 і post-2 (IntersectWith): {collections, csharp}
Теги post-1, яких немає в post-2 (ExceptWith): {dotnet}
Теги post-1 після RemoveTag("dotnet"): {collections, csharp}

Сценарій 3. SearchIndex (Dictionary + HashSet)
Слів в індексі: 13
Search("programming"): {1, 2}
Search("C#"): {1, 3}
SearchMultiple("c#", "java"): {3}
SearchMultiple("programming", "modern"): {1}

Custom comparer
caseInsensitive["HELLO"] = 1
Додано John (25): True, додано John (30): False, усього людей: 1
```
