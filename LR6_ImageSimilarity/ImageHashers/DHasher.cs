using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LR6_ImageSimilarity;

public class DHasher : BaseImageHasher
{
    public override byte[] ComputeHashBytes(Image<Rgba32> image)
    {
        using var clone = image.Clone(ctx => ctx.Resize(9, 8).Grayscale());

        var byteArray = new byte[64];

        clone.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);

                for (var x = 0; x < pixelRow.Length - 1; x++)
                {
                    var current = pixelRow[x].R;
                    var next = pixelRow[x + 1].R;

                    byteArray[y * 8 + x] = (byte)(current < next ? 1 : 0);
                }
            }
        });

        return byteArray;
    }
}