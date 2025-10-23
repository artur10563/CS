using System.Security.Cryptography;
using System.Text;
using System.Numerics;

namespace LR7_DES_RSA
{
    internal abstract class Program
    {
        static void Main()
        {
            var plaintext1 = "Blockchain!";
            var plaintext2 = "Blockchain?";
            var key = GenerateRandomKey();

            using var des = DES.Create();
            des.Key = key;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            Console.WriteLine("Ключ DES: " + BitConverter.ToString(key).Replace("-", ""));


            var encryptor = des.CreateEncryptor();
            var decryptor = des.CreateDecryptor();

            var encrypted1 = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plaintext1), 0, Encoding.UTF8.GetBytes(plaintext1).Length);
            var encrypted2 = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plaintext2), 0, Encoding.UTF8.GetBytes(plaintext2).Length);

            Console.WriteLine("\nВідкритий текст 1: " + plaintext1);
            Console.WriteLine("Шифротекст 1: " + BitConverter.ToString(encrypted1).Replace("-", ""));
            Console.WriteLine("\nВідкритий текст 2: " + plaintext2);

            Console.WriteLine("Шифротекст 2: " + BitConverter.ToString(encrypted2).Replace("-", ""));

            var differingBits = CountDifferingBits(encrypted1, encrypted2);

            Console.WriteLine($"\nКількість відмінних бітів (лавинний ефект): {differingBits} з {encrypted1.Length * 8}");

            var decrypted1 = decryptor.TransformFinalBlock(encrypted1, 0, encrypted1.Length);
            var decrypted2 = decryptor.TransformFinalBlock(encrypted2, 0, encrypted2.Length);

            Console.WriteLine("\nДешифрований текст 1: " + Encoding.UTF8.GetString(decrypted1));
            Console.WriteLine("Дешифрований текст 2: " + Encoding.UTF8.GetString(decrypted2));
        }

        private static byte[] GenerateRandomKey()
        {
            var key = new byte[8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        private static int CountDifferingBits(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException("Arrays must have the same length.");
            return a.Select((val, idx) => (byte)(val ^ b[idx])).Sum(xor => BitOperations.PopCount(xor));
        }
    }
}