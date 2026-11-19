using System;
using System.Collections.Generic;

namespace lab13v6
{
       public class ImageProcessingException : Exception
    {
        public string ImageName { get; }
        public string Operation { get; }

        public ImageProcessingException()
            : base("Виникла помилка під час обробки зображення.")
        {
            ImageName = "Unknown";
            Operation = "Unknown";
        }

        public ImageProcessingException(string message)
            : base(message)
        {
            ImageName = "Unknown";
            Operation = "Unknown";
        }

        public ImageProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
            ImageName = "Unknown";
            Operation = "Unknown";
        }

        public ImageProcessingException(string imageName, string operation, string message)
            : base(message)
        {
            ImageName = imageName;
            Operation = operation;
        }

        public ImageProcessingException(string imageName, string operation, string message, Exception innerException)
            : base(message, innerException)
        {
            ImageName = imageName;
            Operation = operation;
        }
    }

       public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string Error { get; }

        private Result(T? value, bool isSuccess, string error)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(default, false, error ?? "Невідома помилка.");
        }

        public static Result<T> Failure(Exception ex)
        {
            return new Result<T>(default, false, ex?.Message ?? "Виникла виняткова ситуація.");
        }
    }

   
    public class ImageProcessor
    {
        private readonly HashSet<string> _availableFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Grayscale",
            "Sepia",
            "Blur",
            "Sharpen",
            "Contrast"
        };
        private bool IsValidImageHeader(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 4)
                return false;

            bool isPng = imageData[0] == 0x89 && imageData[1] == 0x50;
            bool isJpeg = imageData[0] == 0xFF && imageData[1] == 0xD8;

            return isPng || isJpeg;
        }
        public byte[] ResizeImage(byte[] imageData, int width, int height, string imageName = "photo_sample.png")
        {
            if (width <= 0 || height <= 0)
            {
                throw new ImageProcessingException(
                    imageName,
                    "ResizeImage",
                    $"Некоректні цільові розміри зображення: {width}x{height} px.");
            }

            if (!IsValidImageHeader(imageData))
            {
                throw new ImageProcessingException(
                    imageName,
                    "ResizeImage",
                    "Дані зображення пошкоджені або мають непідтримуваний заголовок файлу.");
            }

            // Імітація створення нового масиву байтів зі зміненим розміром
            byte[] resizedData = new byte[imageData.Length + 8];
            Array.Copy(imageData, resizedData, imageData.Length);
            resizedData[^2] = (byte)(width % 256);
            resizedData[^1] = (byte)(height % 256);

            return resizedData;
        }

       
        public Result<bool> ApplyFilter(byte[] imageData, string filterName)
        {
            if (!IsValidImageHeader(imageData))
            {
                return Result<bool>.Failure("Неможливо застосувати фільтр: дані зображення пошкоджені.");
            }

            if (string.IsNullOrWhiteSpace(filterName) || !_availableFilters.Contains(filterName))
            {
                return Result<bool>.Failure(
                    $"Фільтр '{filterName}' не знайдено. Доступні фільтри: {string.Join(", ", _availableFilters)}.");
            }

            // Імітація успішного застосування фільтра
            return Result<bool>.Success(true);
        }
    }

       internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("  ЛАБОРАТОРНА РОБОТА №13 | ВАРІАНТ 6: Власні винятки та патерн Result");

            var processor = new ImageProcessor();

            // Коректні байти зображення (заголовок PNG + пікселі)
            byte[] validPngImage = { 0x89, 0x50, 0x4E, 0x47, 0x10, 0x20, 0x30, 0x40 };

            // Пошкоджені байти зображення
            byte[] corruptedImage = { 0x00, 0x11 };

        
            Console.WriteLine(" 1. ВЛАСНИЙ ВИНЯТОК: ImageProcessor.ResizeImage (try-catch) ");

            // 1.1. Успішне масштабування коректного зображення
            try
            {
                byte[] resized = processor.ResizeImage(validPngImage, 1920, 1080, "banner_hd.png");
                Console.WriteLine($"  [Успіх] Зображення 'banner_hd.png' масштабовано до 1920x1080. Новий розмір: {resized.Length} байт.");
            }
            catch (ImageProcessingException ex)
            {
                Console.WriteLine($"  [Помилка] {ex.Message}");
            }

            // 1.2. Спроба масштабувати пошкоджене зображення -> викидання ImageProcessingException
            try
            {
                Console.WriteLine("  [Спроба] Масштабування пошкодженого файлу 'broken_avatar.png'...");
                processor.ResizeImage(corruptedImage, 256, 256, "broken_avatar.png");
            }
            catch (ImageProcessingException ex)
            {
                Console.WriteLine($"  [Перехоплено ImageProcessingException]");
                Console.WriteLine($"    • Файл (ImageName)    : {ex.ImageName}");
                Console.WriteLine($"    • Операція (Operation): {ex.Operation}");
                Console.WriteLine($"    • Повідомлення        : {ex.Message}");
            }
            Console.WriteLine();

           
            Console.WriteLine(" 2. ПАТЕРН RESULT<T>: ImageProcessor.ApplyFilter ");

            // 2.1. Застосування існуючого фільтра ("Sepia")
            Result<bool> sepiaResult = processor.ApplyFilter(validPngImage, "Sepia");
            if (sepiaResult.IsSuccess)
            {
                Console.WriteLine($"  [Result.Success] Фільтр 'Sepia' успішно застосовано (Value = {sepiaResult.Value}).");
            }
            else
            {
                Console.WriteLine($"  [Result.Failure] {sepiaResult.Error}");
            }

            // 2.2. Спроба застосувати невідомий фільтр ("CyberpunkNeon") -> контрольована невдача без винятку
            Result<bool> unknownFilterResult = processor.ApplyFilter(validPngImage, "CyberpunkNeon");
            if (unknownFilterResult.IsSuccess)
            {
                Console.WriteLine($"  [Result.Success] Фільтр застосовано.");
            }
            else
            {
                Console.WriteLine($"  [Result.Failure] Очікувана помилка без винятку:");
                Console.WriteLine($"    • IsSuccess: {unknownFilterResult.IsSuccess}");
                Console.WriteLine($"    • Error    : {unknownFilterResult.Error}");
            }

            // 2.3. Демонстрація фабричного методу Result<T>.Failure(Exception)
            Console.WriteLine("\n3. ПЕРЕТВОРЕННЯ ВИНЯТКУ У RESULT (Result<T>.Failure(Exception)) ");
            Result<byte[]> wrappedResult;
            try
            {
                byte[] data = processor.ResizeImage(corruptedImage, 800, 600, "corrupted_scan.jpg");
                wrappedResult = Result<byte[]>.Success(data);
            }
            catch (ImageProcessingException ex)
            {
                wrappedResult = Result<byte[]>.Failure(ex);
            }

            Console.WriteLine($"  [Result.Failure(ex)] IsSuccess = {wrappedResult.IsSuccess} | Error = \"{wrappedResult.Error}\"");

            Console.WriteLine("ВИСНОВОК:");
            Console.WriteLine(" Власні винятки (ImageProcessingException) сигналізують про критичні");
            Console.WriteLine(" збої даних, тоді як Result<T> забезпечує швидку та чисту обробку");
            Console.WriteLine(" очікуваних помилок валідації (невідомий фільтр) без витрат на try-catch.");
        }
    }
}