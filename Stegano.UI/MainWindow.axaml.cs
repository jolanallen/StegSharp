using System;
using Avalonia.Animation;
using Avalonia.Dialogs;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using Stegano.UI.Utils;
namespace Stegano.UI;

using Serilog;
using Stegano.Core;

public partial class MainWindow : Window
{
    public readonly SteganoCore SteganoCore;
    public MainWindow()
    {
        InitializeComponent();
        SteganoCore = new SteganoCore();  // injection du Logger de Serilog dans SteganoCore

        ZoneAffichage.Content = new Dashboard(); 
    }

    public void ChangePage(UserControl newpage)
    {
        ZoneAffichage.Content = newpage;
    }

    private void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        ChangePage(new About());
    }

    async private void OnImportClick(object? sender, RoutedEventArgs e)
    {

        var topLevel = TopLevel.GetTopLevel(this);
        
        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false,
            FileTypeFilter= new[] {FileTypeFilterImageAll.ImageAll}
        });
        if (files.Count != 1)
        {
            return;
        }
        
        var dashboard = ZoneAffichage.Content as Dashboard;

        if(dashboard.EncoderAffichage.Content != null)
        {   
            var afficher = dashboard.EncoderAffichage.Content as Encoder;
            Console.WriteLine($"Importing image: {files[0].Path.AbsolutePath}");
            afficher.imagePath = files[0].Path.AbsolutePath;
            afficher.EncodeImagePreview.Source = new Bitmap(files[0].Path.AbsolutePath);
        }
        if(dashboard.DecoderAffichage.Content != null)
        {   
            var afficher = dashboard.DecoderAffichage.Content as Decoder;
            afficher.imagePath = files[0].Path.AbsolutePath;
            afficher.DecodeImagePreview.Source = new Bitmap(files[0].Path.AbsolutePath);
        }

    }

    private void OnExportClick(object? sender, RoutedEventArgs e)
    {
    }

    public void ShowDashboard()
    {
        ChangePage(new Dashboard());
    }
}