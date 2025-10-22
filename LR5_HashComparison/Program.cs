using System;
using System.Security.Cryptography;
using System.Text;

namespace LR5_HashComparison
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.Write("Введіть своє ім’я: ");
            var name = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Ім’я не може бути порожнім!");
                return;
            }

            var sha256Hash = ComputeSha256(name);
            var md5Hash = ComputeMd5(name);

            Console.WriteLine($"\nВихідний текст: {name}");
            Console.WriteLine($"SHA-256 хеш : {sha256Hash}");
            Console.WriteLine($"MD5 хеш     : {md5Hash}");
        }

        private static string ComputeSha256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static string ComputeMd5(string input)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}