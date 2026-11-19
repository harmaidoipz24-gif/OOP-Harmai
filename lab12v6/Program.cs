using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace lab12v6
{
   
    public class FileProcessor
    {
        private int _attemptCounter = 0;

      
        public string GetLogContent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Шлях до файлу не може бути порожнім.", nameof(path));

            // Імітація критичної помилки доступу (не підлягає повторним спробам)
            if (path.Contains("restricted", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException($"Відмовлено у доступі до системного файлу '{path}'.");

            _attemptCounter++;

            if (_attemptCounter <= 2)
            {
                throw new FileNotFoundException(
                    $"Файл логів '{path}' ще не сформовано або тимчасово недоступний (спроба #{_attemptCounter}).",
                    path);
            }

            return $"[2026-11-19 17:00:01] INFO: Сервіс запущено.\n" +
                   $"[2026-11-19 17:00:14] WARN: Підвищене навантаження CPU (82%).\n" +
                   $"[2026-11-19 17:00:29] INFO: Резервну копію успішно створено.";
        }
    }

       public class NetworkClient
    {
        private int _attemptCounter = 0;

        public void SendLogToServer(string url, string logContent)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL сервера не може бути порожнім.", nameof(url));

            _attemptCounter++;

            if (_attemptCounter <= 4)
            {
                throw new HttpRequestException(
                    $"Не вдалося підключитися до сервера '{url}': 503 Service Unavailable (спроба #{_attemptCounter}).");
            }

            Console.WriteLine($"  [NetworkClient] Логи ({logContent.Length} симв.) успішно доставлено на {url}!");
        }
    }


    public static class RetryHelper
    {
       
        public static T ExecuteWithRetry<T>(
            Func<T> operation,
            int retryCount = 3,
            TimeSpan initialDelay = default,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (retryCount < 0)
                throw new ArgumentOutOfRangeException(nameof(retryCount), "Кількість повторів не може бути від'ємною.");

            if (initialDelay == default)
                initialDelay = TimeSpan.FromMilliseconds(200);

            int retryAttempt = 0;

            while (true)
            {
                try
                {
                    Console.WriteLine($"[RetryHelper] Виконання спроби #{retryAttempt + 1}...");
                    T result = operation();
                    Console.WriteLine($"[RetryHelper] Операція успішно завершилася на спробі #{retryAttempt + 1}.");
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [Помилка] Спроба #{retryAttempt + 1} невдала: [{ex.GetType().Name}] {ex.Message}");

                    // Перевіряємо через делегат shouldRetry, чи слід повторювати для цього типу винятку
                    if (shouldRetry != null && !shouldRetry(ex))
                    {
                        Console.WriteLine($"  [Скасування Retry] Виняток {ex.GetType().Name} не підлягає повторній спробі згідно з політикою shouldRetry.");
                        throw;
                    }

                    // Якщо вичерпано ліміт повторних спроб — прокидаємо виняток далі
                    if (retryAttempt >= retryCount)
                    {
                        Console.WriteLine($"  [Ліміт вичерпано] Досягнуто максимальну кількість повторів ({retryCount}).");
                        throw;
                    }

                    // Експоненційна затримка: initialDelay * 2^retryAttempt
                    double delayMs = initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt);
                    TimeSpan delay = TimeSpan.FromMilliseconds(delayMs);

                    retryAttempt++;
                    Console.WriteLine($"  [Очікування] Пауза {delay.TotalMilliseconds} мс (формула: {initialDelay.TotalMilliseconds} * 2^{retryAttempt - 1}) перед повтором #{retryAttempt}...");
                    Thread.Sleep(delay);
                }
            }
        }

        public static void ExecuteWithRetry(
            Action operation,
            int retryCount = 3,
            TimeSpan initialDelay = default,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            ExecuteWithRetry<bool>(() =>
            {
                operation();
                return true;
            }, retryCount, initialDelay, shouldRetry);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("  ЛАБОРАТОРНА РОБОТА №12 | ВАРІАНТ 6: Обробка помилок та патерн Retry");

           
            // Повторювати виключно для FileNotFoundException та HttpRequestException
            Func<Exception, bool> variant6RetryPolicy = ex =>
                ex is FileNotFoundException || ex is HttpRequestException;

            var fileProcessor = new FileProcessor();
            var networkClient = new NetworkClient();
            string logContent = string.Empty;

           
            Console.WriteLine(" СЦЕНАРІЙ 1: Отримання логів з файлу (FileProcessor.GetLogContent) ");
            try
            {
                logContent = RetryHelper.ExecuteWithRetry(
                    operation: () => fileProcessor.GetLogContent("logs/system_events.log"),
                    retryCount: 3,
                    initialDelay: TimeSpan.FromMilliseconds(200),
                    shouldRetry: variant6RetryPolicy
                );

                Console.WriteLine("\nОтриманий вміст лог-файлу:");
                Console.WriteLine(logContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критичний збій при читанні файлу: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[Блок finally] Завершено етап роботи з файловою системою.\n");
            }

        
            Console.WriteLine("СЦЕНАРІЙ 2: Відправка логів на сервер (NetworkClient.SendLogToServer)");
            try
            {
                // Передаємо retryCount = 4, оскільки перші 4 спроби імітують збій, а 5-та успішна
                RetryHelper.ExecuteWithRetry(
                    operation: () => networkClient.SendLogToServer("https://api.monitoring.local/v1/logs", logContent),
                    retryCount: 4,
                    initialDelay: TimeSpan.FromMilliseconds(150),
                    shouldRetry: variant6RetryPolicy
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критичний збій при відправці логів: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[Блок finally] Мережеву сесію закрито.\n");
            }

          
            Console.WriteLine(" СЦЕНАРІЙ 3: Перевірка shouldRetry (виняток UnauthorizedAccessException) ");
            try
            {
                RetryHelper.ExecuteWithRetry(
                    operation: () => fileProcessor.GetLogContent("C:/Windows/System32/restricted.log"),
                    retryCount: 3,
                    initialDelay: TimeSpan.FromMilliseconds(200),
                    shouldRetry: variant6RetryPolicy
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[Перехоплено в Main] Операцію негайно зупинено без повторів: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[Блок finally] Перевірку політики shouldRetry завершено.\n");
            }

            Console.WriteLine("ВИСНОВОК:");
            Console.WriteLine(" Експоненційна затримка (initialDelay * 2^retryAttempt) подвоює паузу після");
            Console.WriteLine(" кожної невдачі, знижуючи навантаження на ресурс, а делегат shouldRetry");
            Console.WriteLine(" дозволяє миттєво переривати виконання при фатальних помилках доступу.");
        }
    }
}