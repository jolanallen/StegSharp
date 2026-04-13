using Serilog;
using Stegano.Core.DTOs;
using Stegano.Core.Services;

namespace Stegano.Core.Repositories;

public sealed class ImageRepository
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".bmp",
        ".webp"
    };

    private readonly ImageMapper _mapper = new();

    public FileStream OpenReadAsync(string path)
    {   
        if(!File.Exists(path))
            throw new FileNotFoundException("File not found", path);
        if(!AllowedExtensions.Contains(Path.GetExtension(path)))
            throw new ArgumentException("Invalid file format", path);
        try
        {
            FileStream stream=  File.OpenRead(path);
            Log.Debug("Fichier ouvert avec succès : {FilePath}", path);
            if(!stream.CanRead)
                throw new InvalidOperationException("Cannot read from the file", null);
            return stream;
        }catch (Exception)
        {
            Log.Error("Error occurred while opening the file: {FilePath}", path);
            throw new InvalidOperationException("Error occurred while opening the file", null);
        }
    }

    public async Task<MappedImage> OpenMappedAsync(string path, CancellationToken cancellationToken = default)
    {
        using FileStream stream = OpenReadAsync(path);
        return await _mapper.FromStreamAsync(stream, Path.GetExtension(path), cancellationToken);
    }

    public async Task<byte[]> ToPngBytesAsync(MappedImage mappedImage, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        Log.Debug("Conversion de l'image mappée en bytes PNG. Taille de l'image : {Width}x{Height} pixels", mappedImage.Width, mappedImage.Height);
        
        await _mapper.WriteToStreamAsync(mappedImage, memoryStream, ".png", cancellationToken);
        Log.Debug("Image convertie en bytes PNG. Taille des données : {DataSize} octets", memoryStream.Length);
        
        return memoryStream.ToArray();
    }
}
