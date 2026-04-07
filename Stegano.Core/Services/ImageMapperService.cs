using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using Stegano.Core.DTOs;

namespace Stegano.Core.Services;

public sealed class ImageMapper
{
    public async Task<MappedImage> FromStreamAsync(
        Stream source,
        string? sourceExtension = null,
        CancellationToken cancellationToken = default)
    {
        using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(source, cancellationToken);
        int width = image.Width;
        int height = image.Height;
        byte[] pixels = new byte[width * height * 4];

        int rowSize = width * 4;
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                ReadOnlySpan<byte> rowBytes = MemoryMarshal.AsBytes(row);
                rowBytes.CopyTo(pixels.AsSpan(y * rowSize, rowSize));
            }        
        });
        

        return new MappedImage
        {
            Width = width,
            Height = height,
            PixelData = pixels,
            SourceExtension = sourceExtension
        };
    }

    public async Task WriteToStreamAsync(
        MappedImage mapped,
        Stream destination,
        string? outputExtension = ".png",
        CancellationToken cancellationToken = default)
    {
        using var image = new Image<Rgba32>(mapped.Width, mapped.Height);

        int rowSize = mapped.Width * 4;
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < mapped.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                Span<byte> rowBytes = MemoryMarshal.AsBytes(row);
                mapped.PixelData.AsSpan(y * rowSize, rowSize).CopyTo(rowBytes);
            }        
        });

        IImageEncoder encoder = GetEncoder(outputExtension);
        await image.SaveAsync(destination, encoder, cancellationToken);
    }

    private static IImageEncoder GetEncoder(string? extension)
    {
        return extension?.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => new JpegEncoder(),
            ".bmp" => new BmpEncoder(),
            ".gif" => new GifEncoder(),
            ".webp" => new WebpEncoder(),
            _ => new PngEncoder()
        };
    }
}