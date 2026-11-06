# IndependentWork16

Самостійна робота №16.

Власна реалізація інвертованого індексу (`Dictionary<string, HashSet<int>>`) для повнотекстового пошуку та порівняння його продуктивності з лінійним пошуком.

## Структура

| Файл | Вміст |
|---|---|
| `InvertedIndex.cs` | класи `Document` і `InvertedIndex` (`AddDocument`, `SearchSingleWord`, `SearchMultipleWords`, `SearchAnyWord`, `SearchRanked`) |
| `TextProcessor.cs` | токенізація, видалення стоп-слів, спільна нормалізація для обох видів пошуку |
| `PerformanceTest.cs` | порівняння лінійного пошуку та індексу через `Stopwatch` |
| `Program.cs` | тестові документи, демонстрація, вимірювання на 10/100/1000/10000 документів, інтерактивний пошук |
| `REPORT.md` | опис реалізації, таблиця вимірювань, висновки |

## Приклад виводу

Початок роботи програми (індекс і пошук):

```
Loaded 5 documents
Index contains 31 unique words

Демонстрація пошуку
SearchSingleWord("python"): 3, 5
SearchSingleWord("C#"): 1, 4, 5
SearchMultipleWords("programming", "language") — AND: 1, 2, 3
SearchMultipleWords("c#", "modern") — AND: 1, 4
SearchAnyWord("java", "python") — OR: 2, 3, 5
SearchSingleWord("rust") — немає в індексі: нічого не знайдено
SearchRanked("c#", "java", "popular") — OR з ранжуванням:
   [5] Comparing Languages — збігів: 3
   [2] Java Basics — збігів: 2
   [1] Introduction to C# — збігів: 1
   [4] C# Advanced Features — збігів: 1
```

Блок вимірювань (час залежить від комп'ютера, тому тут показано лише формат):

```
Вимірювання продуктивності (слово для пошуку: "python")
Час — середній на один пошук; побудова індексу — один раз для всієї колекції.

10 документів:
   Linear search:  ... ticks (... мкс)
   Inverted Index: ... ticks (... мкс)
   Побудова індексу: ... мс
   Linear found: 4, Index found: 4 (збігається)
...
10000 документів:
   ...
   Linear found: 4000, Index found: 4000 (збігається)
```

Інтерактивний пошук:

```
Enter search query (or 'exit'): programming language

Search Results (AND)
[1] Introduction to C#
  C# is a modern object-oriented programming language developed by Microsoft.
[2] Java Basics
  Java is a popular programming language known for its portability and performance.
[3] Python Overview
  Python is a high-level programming language with simple syntax.

Found 3 documents

Enter search query (or 'exit'): c# async

Search Results (AND)
[4] C# Advanced Features
  C# supports LINQ, async/await, and many modern programming paradigms.

Found 1 documents

Enter search query (or 'exit'): exit
```
