using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Stegano.UI.Converters;
using System.ComponentModel;
using Avalonia.Media.Imaging;
using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Stegano.Core.DTOs;
using System.IO;
using Avalonia.Platform.Storage;
using Stegano.UI.Utils;
using Avalonia.Input;
using System.Linq;

namespace Stegano.UI;

public partial class Decoder : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;
    public string imagePath = string.Empty;
    private readonly Core.Services.DecodeService _decodeService;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value) return;
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage = "Prêt.";
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    private string _messageUsageInfo = "Texte caché: 0 caractère / 0 octet";
    public string MessageUsageInfo
    {
        get => _messageUsageInfo;
        private set
        {
            if (_messageUsageInfo == value) return;
            _messageUsageInfo = value;
            OnPropertyChanged();
        }
    }

    private bool _hasNoDecodedMessage = true;
    public bool HasNoDecodedMessage
    {
        get => _hasNoDecodedMessage;
        private set
        {
            if (_hasNoDecodedMessage == value) return;
            _hasNoDecodedMessage = value;
            OnPropertyChanged();
        }
    }

    public Decoder()
    {
        InitializeComponent();
        DataContext = this;

        DropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        DropZone.AddHandler(DragDrop.DropEvent, OnDrop);
        DropZone.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);

        _decodeService = new();
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files) || e.Data.Contains(DataFormats.FileNames))
        {
            e.DragEffects = DragDropEffects.Copy;
            DropZone.BorderBrush = Avalonia.Media.Brushes.LightBlue;
            DropZone.BorderThickness = new Thickness(2);
            e.Handled = true;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnDragLeave(object? sender, RoutedEventArgs e)
    {
        DropZone.BorderBrush = Avalonia.Media.Brushes.Gray;
        DropZone.BorderThickness = new Thickness(1);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        DropZone.BorderBrush = Avalonia.Media.Brushes.Gray;
        DropZone.BorderThickness = new Thickness(1);

        string? path = null;

        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null && files.Any())
            {
                path = files.First().Path.AbsolutePath;
            }
        }
        else if (e.Data.Contains(DataFormats.FileNames))
        {
            var fileNames = e.Data.GetFileNames();
            if (fileNames != null && fileNames.Any())
            {
                path = fileNames.First();
            }
        }

        if (!string.IsNullOrEmpty(path))
        {
            imagePath = path;
            try
            {
                DecodeImagePreview.Source = new Bitmap(imagePath);
                DecodeResultBox.Text = string.Empty;
                HasNoDecodedMessage = true;
                MessageUsageInfo = "Texte caché: 0 caractère / 0 octet";
                StatusMessage = $"Image chargée via drag-drop: {Path.GetFileName(imagePath)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement de l'image: {ex.Message}";
            }
        }
    }

    public async void OnSelectImageClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            StatusMessage = "Impossible d'ouvrir le sélecteur d'image.";
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Sélectionner une image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FileTypeFilterImageAll.ImageAll }
        });

        if (files.Count != 1)
        {
            StatusMessage = "Aucune image sélectionnée.";
            return;
        }

        imagePath = files[0].Path.AbsolutePath;
        DecodeImagePreview.Source = new Bitmap(imagePath);
        DecodeResultBox.Text = string.Empty;
        HasNoDecodedMessage = true;
        MessageUsageInfo = "Texte caché: 0 caractère / 0 octet";
        StatusMessage = $"Image chargée: {Path.GetFileName(imagePath)}";
    }

    public async void OnDecodeClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            StatusMessage = "Importez une image avant de lancer le décodage.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Décodage en cours...";

        try
        {
            DecodeResult result = await _decodeService.DecodeAsync(new DecodeRequest(imagePath));
            DecodeResultBox.Text = result.Message;
            HasNoDecodedMessage = string.IsNullOrWhiteSpace(result.Message);
            UpdateMessageUsage(result.Message);
            StatusMessage = "Décodage terminé.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors du décodage: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async void OnCopyTextClick(object? sender, RoutedEventArgs e)
    {
        var message = DecodeResultBox.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(message))
        {
            StatusMessage = "Aucun texte à copier.";
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is null)
        {
            StatusMessage = "Impossible d'accéder au presse-papiers.";
            return;
        }

        await topLevel.Clipboard.SetTextAsync(message);
        StatusMessage = "Texte copié dans le presse-papiers.";
    }

    private void UpdateMessageUsage(string message)
    {
        MessageUsageInfo = $"Texte caché: {message.Length} caractère(s) / {Encoding.UTF8.GetByteCount(message)} octet(s)";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}