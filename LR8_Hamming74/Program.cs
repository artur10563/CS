namespace LR8_Hamming74;

public class Hamming74
{
    static int[] Encode(int[] data)
    {
        // data: 4 bits
        var code = new int[7];

        // Data bits
        code[2] = data[0]; // d1
        code[4] = data[1]; // d2
        code[5] = data[2]; // d3
        code[6] = data[3]; // d4

        // Parity bits
        code[0] = code[2] ^ code[4] ^ code[6]; // p1
        code[1] = code[2] ^ code[5] ^ code[6]; // p2
        code[3] = code[4] ^ code[5] ^ code[6]; // p3

        return code;
    }

    static int[] Decode(int[] code)
    {
        // Calculate syndrome
        var s1 = code[0] ^ code[2] ^ code[4] ^ code[6];
        var s2 = code[1] ^ code[2] ^ code[5] ^ code[6];
        var s3 = code[3] ^ code[4] ^ code[5] ^ code[6];

        var errorPos = s3 * 4 + s2 * 2 + s1; // 1-based

        if (errorPos != 0)
        {
            Console.WriteLine($"Error at position {errorPos}, correcting...");
            code[errorPos - 1] ^= 1;
        }

        return [code[2], code[4], code[5], code[6]];
    }


    class Program
    {
        static void Main(string[] args)
        {
            int[] data = { 1, 0, 1, 1 };
            Console.WriteLine("Original data: " + string.Join("", data));

            int[] encoded = Encode(data);
            Console.WriteLine("Encoded: " + string.Join("", encoded));

            // Introduce single-bit error
            encoded[4] ^= 1;
            Console.WriteLine("With error: " + string.Join("", encoded));

            int[] decoded = Decode(encoded);
            Console.WriteLine("Decoded: " + string.Join("", decoded));
        }
    }
}