using Stegano.Core.DTOs;
using Stegano.Core.Repositories;
using System.Text;

namespace Stegano.Core.Services;

public sealed class DecodeService
{
    private readonly ImageRepository _imageRepository;
    private readonly LsbSteganographyService _lsbSteganographyService;

    public DecodeService()
    {
        _imageRepository = new ImageRepository();
        _lsbSteganographyService = new LsbSteganographyService();
    }

    public async Task<DecodeResult> DecodeAsync(DecodeRequest request, CancellationToken cancellationToken = default)
    {
        MappedImage mapped = await _imageRepository.OpenMappedAsync(request.SourceImagePath, cancellationToken);
        string message = await _lsbSteganographyService.ExtractAsync(mapped.PixelData, cancellationToken);
        return new DecodeResult(message, true);
    }

}
