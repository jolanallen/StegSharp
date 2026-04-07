using Stegano.Core.DTOs;
using Stegano.Core.Repositories;
using System.Text;

namespace Stegano.Core.Services;

public sealed class LsbSteganographyService
{
    private readonly ImageRepository _imageRepositry = new();
    public Task<byte[]> EncodeAsync(byte[] pixels, string message, CancellationToken cancellationToken = default)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
        byte[] payload = new byte[lengthBytes.Length + messageBytes.Length];

        Buffer.BlockCopy(lengthBytes, 0, payload, 0, lengthBytes.Length);
        Buffer.BlockCopy(messageBytes, 0, payload, lengthBytes.Length, messageBytes.Length);
        
        int currentPixelsIdx = 0;
        for (var i = 0; i < payload.Length; i++)
        {
            for (var y = 0; y < 8; y++)
            {
                pixels[currentPixelsIdx] = this.SetLSB(pixels[currentPixelsIdx], (payload[i] >> y) & 1);
                currentPixelsIdx++;
            }
        }
        return Task.FromResult(pixels);
    }

    public Task<string> ExtractAsync(byte[] pixels, CancellationToken cancellationToken = default)
    {
        byte[] lengthBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            byte currentByte = 0;
            for (int bit = 0; bit < 8; bit++)
            {
                int pixelIndex = (i * 8) + bit;
                currentByte |= (byte)((pixels[pixelIndex] & 1) << bit);
            }

            lengthBytes[i] = currentByte;
        }

        int messageLength = BitConverter.ToInt32(lengthBytes, 0);
        if (messageLength < 0)
        {
            throw new InvalidOperationException("Invalid hidden message length.");
        }

        byte[] messageBytes = new byte[messageLength];
        int startingPixelIndex = 32;

        for (int i = 0; i < messageLength; i++)
        {
            byte currentByte = 0;
            for (int bit = 0; bit < 8; bit++)
            {
                int pixelIndex = startingPixelIndex + (i * 8) + bit;
                currentByte |= (byte)((pixels[pixelIndex] & 1) << bit);
            }

            messageBytes[i] = currentByte;
        }

        return Task.FromResult(Encoding.UTF8.GetString(messageBytes));
    }

    public int GetCapacity(MappedImage sourceImage, CancellationToken cancellationToken = default)
    {
        // 1 bit hiden in 1 byte, so 1byte hide in 8byte (1byte equal 8bits)
        // -4 to reserve 4 bytes for the message length header
        return (sourceImage.PixelData.Length / 8) - 4;
    }

    public byte SetLSB(byte value, int bit)
    {
        return (byte)((value & 0b11111110) | bit);
    }

}
