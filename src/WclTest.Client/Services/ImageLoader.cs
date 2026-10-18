using System.IO;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace WclTest.Client.Services;

public sealed class ImageLoader : IImageLoader
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private readonly ILogger<ImageLoader> _logger;

    public ImageLoader(ILogger<ImageLoader> logger) => _logger = logger;

    public async Task<Bitmap?> LoadAsync(string? url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        try
        {
            var bytes = await Http.GetByteArrayAsync(url, ct);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Avatar load failed for {Url}", url);
            return null;
        }
    }
}
