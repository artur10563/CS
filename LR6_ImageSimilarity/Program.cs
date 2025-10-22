using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LR6_ImageSimilarity;

internal class Program
{
    private static readonly string BaseDir = AppContext.BaseDirectory;
    private static readonly string InputImagesDir = Path.Combine(BaseDir, @"..\..\..\TestImages\InputImages");
    private static readonly string OutputImagesDir = Path.Combine(BaseDir, @"..\..\..\TestImages\OutputImages");


    static async Task Main(string[] args)
    {
        var imagePaths = Directory.GetFiles(InputImagesDir, "*.png");
        if (Directory.Exists(OutputImagesDir)) Directory.Delete(OutputImagesDir, recursive: true);
        Directory.CreateDirectory(OutputImagesDir);

        const int similarityThreshold = 20;

        var hashers = new IImageHasher[]
        {
            new AHasher(),
            new DHasher()
        };

        //Image hash, similar images
        var imageHashes = new Dictionary<string, List<ulong>>();
        
        foreach (var imagePath in imagePaths)
        {
            var imageName = Path.GetFileNameWithoutExtension(imagePath);

            using var original = await Image.LoadAsync<Rgba32>(imagePath);

            foreach (var hasher in hashers)
            {
                var hashBytes = hasher.ComputeHashBytes(original);
                var ulongHash = BaseImageHasher.BytesToUlong(hashBytes);

                if (!imageHashes.TryGetValue(imageName, out var hashList))
                    imageHashes.Add(imageName, [ulongHash]);
                else
                    hashList.Add(ulongHash);

                var image = BaseImageHasher.BytesToImage(hashBytes);
                var fileName = $"{imageName}_{hasher.GetType().Name}_{BaseImageHasher.BytesToHex(hashBytes)}";
                await image.SaveAsPngAsync($"{OutputImagesDir}/{fileName}.png");
            }
        }

        
        var imageNames = imageHashes.Keys.ToList();

        for (var i = 0; i < imageNames.Count; i++)
        {
            for (var j = i + 1; j < imageNames.Count; j++)
            {
                var img1 = imageNames[i];
                var img2 = imageNames[j];

                var hashes1 = imageHashes[img1];
                var hashes2 = imageHashes[img2];

                var totalDistance = hashes1.Select((hash1, index) => BaseImageHasher.HammingDistance(hash1, hashes2[index])).Sum();
                var avgDistance = totalDistance / (double)hashes1.Count;

                if (avgDistance < similarityThreshold)
                {
                    Console.WriteLine($"Similar: {img1} <-> {img2} | Avg Distance = {avgDistance}");
                }
            }
        }
    }
}