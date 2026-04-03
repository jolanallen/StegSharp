using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Stegano.UI.Converters;

namespace Stegano.UI;

public partial class Decoder : UserControl
{
    public Decoder()
    {
        InitializeComponent();
    }

     private void OnDecodeClick(object? sender, RoutedEventArgs e)
    {
        // Logiqued'appel à Stagano.Core pour encoder le texte dans l'image
    }
}