using System;

namespace OOP_Student
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine(" Сценарій 1: Використання block using ");
            using (var player1 = new AudioPlayer("Song 1 - Bohemian Rhapsody"))
            {
                player1.Play();
                player1.Stop();
            } // Dispose() викликається детерміновано та автоматично під час виходу з блоку

            Console.WriteLine("\n Сценарій 2: Явний виклик Dispose() ");
            var player2 = new AudioPlayer("Song 2 - Hotel California");
            player2.Play();
            player2.Dispose(); // Ручне детерміноване звільнення ресурсів

            Console.WriteLine("\n Сценарій 3: Без Dispose() (фіналізація через GC) ");
            CreateUnmanagedPlayer();

            // Примусовий виклик GC для демонстрації фіналізатора
            Console.WriteLine("Запуск GC.Collect()...");
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("\nРоботу програми завершено.");
        }

        static void CreateUnmanagedPlayer()
        {
            var player3 = new AudioPlayer("Song 3 - Stairway to Heaven");
            player3.Play();
            // Об'єкт залишає область видимості без виклику Dispose()
        }
    }
}