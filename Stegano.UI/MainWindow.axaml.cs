using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;

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

    private void OnImportClick(object? sender, RoutedEventArgs e)
    {
    }

    private void OnExportClick(object? sender, RoutedEventArgs e)
    {
    }

    public void ShowDashboard()
    {
        ChangePage(new Dashboard());
    }
}