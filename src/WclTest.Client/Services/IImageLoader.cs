using Avalonia.Media.Imaging;

namespace WclTest.Client.Services;

public interface IImageLoader
{
    Task<Bitmap?> LoadAsync(string? url, CancellationToken ct = default);
}
