using System.Collections.Immutable;
using Penn.Content;
using Penn.Pipeline;
using Penn.Routing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace RecipeExample;

public interface IResponsiveImageContentService
{
    Task<byte[]?> ProcessImageAsync(string filename, string size);
    Task<byte[]?> GenerateLqipAsync(string filename);
    Task<(int width, int height)?> GetOriginalImageDimensionsAsync(string filename);
}

public class ResponsiveImageContentService : IResponsiveImageContentService, IContentService
{
    public string DefaultSection => "";
    public int SearchPriority => 5;

    private readonly string _recipePath;
    private static readonly string[] AllSizes = ["lqip", "xs", "sm", "md", "lg", "xl"];

    public ResponsiveImageContentService(string recipePath)
    {
        _recipePath = recipePath;
    }

    public (int width, int height) GetImageDimensions(string size, int originalWidth = 0, int originalHeight = 0)
    {
        var maxWidth = size.ToLowerInvariant() switch
        {
            "lqip" => 40, "xs" => 480, "sm" => 768,
            "md" => 1024, "lg" => 1440, "xl" => 1920,
            "full" => 0, _ => 1024
        };

        if (maxWidth == 0 || originalWidth == 0 || originalHeight == 0)
        {
            return size.ToLowerInvariant() switch
            {
                "lqip" => (40, 30), "xs" => (480, 360), "sm" => (768, 576),
                "md" => (1024, 768), "lg" => (1440, 1080), "xl" => (1920, 1440),
                "full" => (0, 0), _ => (1024, 768)
            };
        }

        var aspectRatio = (double)originalWidth / originalHeight;
        var calculatedHeight = (int)(maxWidth / aspectRatio);
        return (maxWidth, calculatedHeight);
    }

    public async Task<byte[]?> ProcessImageAsync(string filename, string size)
    {
        var sourcePath = Path.Combine(_recipePath, $"{filename}.webp");
        if (!File.Exists(sourcePath)) return null;

        try
        {
            await using var sourceStream = File.OpenRead(sourcePath);
            using var image = await Image.LoadAsync(sourceStream);

            var dimensions = GetImageDimensions(size, image.Width, image.Height);
            if (dimensions is { width: > 0, height: > 0 })
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(dimensions.width, dimensions.height),
                    Mode = ResizeMode.Max
                }));
            }

            using var memoryStream = new MemoryStream();
            var encoder = new WebpEncoder
            {
                UseAlphaCompression = true,
                FileFormat = WebpFileFormatType.Lossy,
                FilterStrength = 60,
                Method = WebpEncodingMethod.Level4,
                Quality = size == "lqip" ? 20 : 75,
            };

            if (size == "lqip")
                image.Mutate(x => x.GaussianBlur(2f));

            await image.SaveAsWebpAsync(memoryStream, encoder);
            return memoryStream.ToArray();
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> GenerateLqipAsync(string filename)
        => await ProcessImageAsync(filename, "lqip");

    public async Task<(int width, int height)?> GetOriginalImageDimensionsAsync(string filename)
    {
        var sourcePath = Path.Combine(_recipePath, $"{filename}.webp");
        if (!File.Exists(sourcePath)) return null;

        try
        {
            await using var sourceStream = File.OpenRead(sourcePath);
            using var image = await Image.LoadAsync(sourceStream);
            return (image.Width, image.Height);
        }
        catch
        {
            return null;
        }
    }

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        if (!Directory.Exists(_recipePath)) yield break;

        var imageFiles = Directory.GetFiles(_recipePath, "*.webp");
        foreach (var imagePath in imageFiles)
        {
            var filename = Path.GetFileNameWithoutExtension(imagePath);
            foreach (var size in AllSizes)
            {
                var url = $"/images/{filename}-{size}.webp";
                var route = new ContentRoute
                {
                    CanonicalPath = new UrlPath(url),
                    OutputFile = new FilePath($"images/{filename}-{size}.webp"),
                };
                ContentSource source = new RazorPageSource(url);
                yield return new DiscoveredItem(route, source);
            }
        }

        await Task.CompletedTask;
    }

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
}
