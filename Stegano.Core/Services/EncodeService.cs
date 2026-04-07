using Stegano.Core.DTOs;
using Stegano.Core.Repositories;
using System.Text;

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

        MappedImage mapped = await _imageRepository.OpenMappedAsync(request.SourceImagePath, cancellationToken);
        int maxLength = _lsbSteganographyService.GetCapacity(mapped, cancellationToken);
        int messageLengthInBytes = Encoding.UTF8.GetByteCount(request.Message);
        if (messageLengthInBytes > maxLength)
        {
            throw new ArgumentException("Message is too long for the selected image.");
        }


        await _lsbSteganographyService.EncodeAsync(mapped.PixelData, request.Message, cancellationToken);
        return new EncodeResult(request.OutputImagePath, true);
    }

    public async Task<EncodePreviewResult> EncodePreviewAsync(EncodeRequest request, CancellationToken cancellationToken = default)
    {
        MappedImage mapped = await _imageRepository.OpenMappedAsync(request.SourceImagePath, cancellationToken);
        int maxLength = _lsbSteganographyService.GetCapacity(mapped, cancellationToken);
        int messageLengthInBytes = Encoding.UTF8.GetByteCount(request.Message);

        if (messageLengthInBytes > maxLength)
        {
            throw new ArgumentException("Message is too long for the selected image.");
        }

        await this._lsbSteganographyService.EncodeAsync(mapped.PixelData, request.Message, cancellationToken);
        byte[] pngBytes = await this._imageRepository.ToPngBytesAsync(mapped, cancellationToken);
        return new EncodePreviewResult(pngBytes, true);
    }

    public async Task<int> GetMaxInputLength(string sourceImagePath, CancellationToken cancellationToken = default)
    {
        MappedImage img = await _imageRepository.OpenMappedAsync(sourceImagePath, cancellationToken);
        return _lsbSteganographyService.GetCapacity(img, cancellationToken);
    }
}
