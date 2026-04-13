using Stegano.Core.DTOs;
using Stegano.Core.Repositories;

using System.Text;
using Serilog;


namespace Stegano.Core.Services;

public sealed class EncodeService
{
    private readonly ImageRepository _imageRepository;
    private readonly LsbSteganographyService _lsbSteganographyService;
    
    public EncodeService()
    {


        _imageRepository = new ImageRepository();
        _lsbSteganographyService = new LsbSteganographyService();

    }

    public async Task<EncodeResult> EncodeAsync(EncodeRequest request, CancellationToken cancellationToken = default)
    {
        Log.Information("Démarrage de l'encodage du message dans l'image : {SourceImagePath}", request.SourceImagePath);
        MappedImage mapped = await _imageRepository.OpenMappedAsync(request.SourceImagePath, cancellationToken);

        int maxLength = _lsbSteganographyService.GetCapacity(mapped, cancellationToken);
        int messageLengthInBytes = Encoding.UTF8.GetByteCount(request.Message);

        Log.Debug("Capacité max : {MaxLength} octets | Taille du message : {MessageLength} octets", maxLength, messageLengthInBytes);
        
        
        if (messageLengthInBytes > maxLength)
        {
            Log.Warning("Échec de l'encodage : Le message ({MessageLength} octets) est trop grand pour l'image sélectionnée (Max: {MaxLength} octets).", messageLengthInBytes, maxLength);
            throw new ArgumentException("Message is too long for the selected image.");
        }

        Log.Information("Taille validée. Insertion du message dans les pixels en cours...");
        await _lsbSteganographyService.EncodeAsync(mapped.PixelData, request.Message, cancellationToken);

        Log.Information("Message encodé avec succès. Enregistrement de l'image de sortie : {OutputImagePath}", request.OutputImagePath);
        return new EncodeResult(request.OutputImagePath, true);
    }

    public async Task<EncodePreviewResult> EncodePreviewAsync(EncodeRequest request, CancellationToken cancellationToken = default)
    {
        MappedImage mapped = await _imageRepository.OpenMappedAsync(request.SourceImagePath, cancellationToken);

        int maxLength = _lsbSteganographyService.GetCapacity(mapped, cancellationToken);
        int messageLengthInBytes = Encoding.UTF8.GetByteCount(request.Message);

        Log.Debug("Génération de l'aperçu d'encodage. Capacité max : {MaxLength} octets | Taille du message : {MessageLength} octets", maxLength, messageLengthInBytes);

        if (messageLengthInBytes > maxLength)
        {
            Log.Warning("Aperçu d'encodage impossible : Le message ({MessageLength} octets) est trop grand pour l'image sélectionnée (Max: {MaxLength} octets).", messageLengthInBytes, maxLength);
            throw new ArgumentException("Message is too long for the selected image.");
        }

        await this._lsbSteganographyService.EncodeAsync(mapped.PixelData, request.Message, cancellationToken);
        Log.Information("Aperçu d'encodage généré avec succès pour l'image : {SourceImagePath}", request.SourceImagePath);

        byte[] pngBytes = await this._imageRepository.ToPngBytesAsync(mapped, cancellationToken);
        Log.Debug("Aperçu d'encodage converti en PNG. Taille du fichier : {PngSize} octets", pngBytes.Length);

        return new EncodePreviewResult(pngBytes, true);
    }

    public async Task<int> GetMaxInputLength(string sourceImagePath, CancellationToken cancellationToken = default)
    {
        MappedImage img = await _imageRepository.OpenMappedAsync(sourceImagePath, cancellationToken);
        Log.Debug("Calcul de la capacité maximale pour l'image : {SourceImagePath}", sourceImagePath);
        return _lsbSteganographyService.GetCapacity(img, cancellationToken);
    }
}
