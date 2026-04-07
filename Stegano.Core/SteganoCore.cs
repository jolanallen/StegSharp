using Stegano.Core.Services;

namespace Stegano.Core;

public sealed class SteganoCore
{
    public EncodeService EncodeService { get; }
    public DecodeService DecodeService { get; }

    public SteganoCore()
    {
        EncodeService = new EncodeService();
        DecodeService = new DecodeService();
    }
}
