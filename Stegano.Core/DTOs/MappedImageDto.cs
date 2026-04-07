namespace Stegano.Core.DTOs;

public sealed class MappedImage
{
    public required int Width { get; init; }
    public required int Height { get; init; }

    // RGBA, 4 bytes per pixel
    public required byte[] PixelData { get; init; }

    // Optional: keep original extension/format info
    public string? SourceExtension { get; init; }
}