namespace Stegano.Core.DTOs;

public sealed record EncodeRequest(string SourceImagePath, string Message, string OutputImagePath);
