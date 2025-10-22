using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LR6_ImageSimilarity;

public interface IImageHasher
{
    byte[] ComputeHashBytes(Image<Rgba32> image);
}

public abstract class BaseImageHasher : IImageHasher
{
    public abstract byte[] ComputeHashBytes(Image<Rgba32> image);

    public static string BytesToHex(byte[] bits) => BytesToUlong(bits).ToString("x16");

    public static ulong BytesToUlong(byte[] bytes)
    {
        if (bytes.Length != 64)
            throw new ArgumentException("Hash must be 64 bytes.");

        ulong hash = 0;
        for (var i = 0; i < 64; i++)
            if (bytes[i] == 1)
                hash |= 1UL << (63 - i);
        return hash;
    }

    public static Image<Rgba32> BytesToImage(byte[] hash)
    {
        if (hash.Length != 64)
            throw new ArgumentException($"Hash must be 64 bytes (8x8).");

        var img = new Image<Rgba32>(8, 8);

        img.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);

                for (var x = 0; x < pixelRow.Length; x++)
                {
                    var b = hash[y * 8 + x];
                    var val = (byte)(b == 1 ? 255 : 0); // white or black
                    pixelRow[x] = new Rgba32(val, val, val);
                }
            }
        });

        return img;
    }

    public static int HammingDistance(ulong hash1, ulong hash2) =>
        BitOperations.PopCount(hash1 ^ hash2);
}