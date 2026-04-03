using Avalonia.Animation;
using Avalonia.Dialogs;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;

namespace Stegano.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
            FileTypeFilter= [new FilePickerFileType("Image Files") ]
        });
        if (files.Count != 1)
        {
            return;
        }
        
        var dashboard = ZoneAffichage.Content as Dashboard;

        if(dashboard.EncoderAffichage.Content != null)
        {   
            var afficher = dashboard.EncoderAffichage.Content as Encoder;
            afficher.EncodeImagePreview.Source = new Bitmap(files[0].Path.AbsolutePath);
        }
        if(dashboard.DecoderAffichage.Content != null)
        {   
            var afficher = dashboard.DecoderAffichage.Content as Decoder;
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