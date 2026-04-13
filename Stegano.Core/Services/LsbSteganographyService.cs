using Serilog;
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
        Log.Debug("Préparation du payload pour l'encodage. Taille du message : {MessageLength} octets | Taille totale du payload : {PayloadLength} octets", messageBytes.Length, payload.Length);

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
        Log.Debug("Payload encodé dans les pixels. Nombre total de pixels modifiés : {ModifiedPixelsCount}", currentPixelsIdx);
        return Task.FromResult(pixels);
    }

    public Task<string> ExtractAsync(byte[] pixels, CancellationToken cancellationToken = default)
    {
        byte[] lengthBytes = new byte[4];
        Log.Debug("Début de l'extraction du message caché. Extraction de la longueur du message en cours...");

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
        Log.Debug("Longueur du message extraite : {MessageLength} octets", BitConverter.ToInt32(lengthBytes, 0));

        int messageLength = BitConverter.ToInt32(lengthBytes, 0);
        if (messageLength < 0)
        {
            Log.Error("Longueur du message extraite invalide : {MessageLength} octets", messageLength);
            throw new InvalidOperationException("Invalid hidden message length.");
        }

        byte[] messageBytes = new byte[messageLength];
        Log.Debug("Extraction du message caché en cours... Nombre de pixels à lire : {PixelsToRead}", messageLength * 8);

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
        Log.Debug("Message caché extrait avec succès. Taille du message : {MessageLength} octets", messageBytes.Length);
        return Task.FromResult(Encoding.UTF8.GetString(messageBytes));
    }

    public int GetCapacity(MappedImage sourceImage, CancellationToken cancellationToken = default)
    {
        // 1 bit hiden in 1 byte, so 1byte hide in 8byte (1byte equal 8bits)
        // -4 to reserve 4 bytes for the message length header
        Log.Debug("Calcul de la capacité maximale d'encodage pour l'image. Taille des données pixel : {PixelDataSize} octets", sourceImage.PixelData.Length);
        return (sourceImage.PixelData.Length / 8) - 4;
    }

    public byte SetLSB(byte value, int bit)
    {
        Log.Debug("Modification du pixel pour l'encodage. Valeur originale : {OriginalValue} | Bit à encoder : {Bit} | Nouvelle valeur : {NewValue}", value, bit, (byte)((value & 0b11111110) | bit));
        return (byte)((value & 0b11111110) | bit);
    }

}
