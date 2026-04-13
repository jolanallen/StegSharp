using Serilog;
using Stegano.Core.Services;


namespace Stegano.Core;

public sealed class SteganoCore
{

    public EncodeService EncodeService { get; }
    public DecodeService DecodeService { get; }

    public SteganoCore()
    {
        Log.Logger = Serilog.Log.Logger;  // Utiliser le logger global de Serilog

        Log.Debug("Initialisation de SteganoCore et de ses services...");
        try
        {
            EncodeService = new EncodeService();
            DecodeService = new DecodeService();
        
            Log.Information("SteganoCore initialisé avec succès.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Échec de l'initialisation de SteganoCore.");
            throw;   // relancer l'exception après l'avoir logué pour informer l'UI
        }
        
    }
}
