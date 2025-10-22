using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LR6_ImageSimilarity;

public class AHasher : BaseImageHasher
{
    public override byte[] ComputeHashBytes(Image<Rgba32> image)
    {
        using var clone = image.Clone(ctx => ctx.Resize(8, 8).Grayscale());
        var pixelArray = new Rgba32[64];
        clone.CopyPixelDataTo(pixelArray);

        var average = pixelArray.Average(x => x.R);
        var hash = pixelArray.Select(x => (byte)(x.R >= average ? 1 : 0)).ToArray();

        return hash;
    }
}