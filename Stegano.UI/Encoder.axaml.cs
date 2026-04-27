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
using Stegano.Core.Services;
using Avalonia.Input;
using System.Linq;

namespace Stegano.UI;

public partial class Encoder : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;
    public string imagePath = "";
    private byte[]? _encodedPreviewBytes;
    private readonly EncodeService _encodeService;

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
    public Encoder()
    {
        InitializeComponent();

        DropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        DropZone.AddHandler(DragDrop.DropEvent, OnDrop);
        DropZone.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);

        // Recompute capacity when image changes
        EncodeImagePreview.PropertyChanged += async (_, e) =>
        {
            if (e.Property == Image.SourceProperty)
                await RecomputeMaxLengthFromCurrentImage();
        };
        _encodeService = new();
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

    private async void OnDrop(object? sender, DragEventArgs e)
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
                EncodeImagePreview.Source = new Bitmap(imagePath);
                _encodedPreviewBytes = null;
                StatusMessage = $"Image chargée via drag-drop: {Path.GetFileName(imagePath)}";
                await RecomputeMaxLengthFromCurrentImage();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement de l'image: {ex.Message}";
            }
        }
    }

    private async void OnSelectImageClick(object? sender, RoutedEventArgs e)
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
        EncodeImagePreview.Source = new Bitmap(imagePath);
        _encodedPreviewBytes = null;
        StatusMessage = $"Image chargée: {Path.GetFileName(imagePath)}";
        await RecomputeMaxLengthFromCurrentImage();
    }


   
    private async void OnEncodeClick(object? sender, RoutedEventArgs e)
    {   
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            StatusMessage = "Importez une image avant de lancer l'encodage.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Encodage en cours...";

        try
        {
            EncodeRequest dto = new(imagePath, EncodeDataBox.Text ?? string.Empty, string.Empty);
            var result = await _encodeService.EncodePreviewAsync(dto);
            _encodedPreviewBytes = result.PngBytes;
            using var ms = new MemoryStream(result.PngBytes);
            EncodeImagePreview.Source = new Bitmap(ms);
            StatusMessage = "Encodage terminé. Vous pouvez enregistrer l'image.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de l'encodage: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (_encodedPreviewBytes is null || _encodedPreviewBytes.Length == 0)
        {
            StatusMessage = "Aucune image encodée à enregistrer.";
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            StatusMessage = "Impossible d'ouvrir la boîte d'enregistrement.";
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Enregistrer l'image encodée",
            SuggestedFileName = $"{Path.GetFileNameWithoutExtension(imagePath)}_stego.png",
            DefaultExtension = "png",
            FileTypeChoices = new[] { FileTypeFilterImageAll.ImageAll }
        });

        if (file is null)
        {
            StatusMessage = "Enregistrement annulé.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Enregistrement en cours...";

        try
        {
            await using var stream = await file.OpenWriteAsync();
            await stream.WriteAsync(_encodedPreviewBytes);
            StatusMessage = $"Image enregistrée: {file.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de l'enregistrement: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private int _maxMessageLength;
    public int MaxMessageLength
    {
        get => _maxMessageLength;
        private set
        {
            if (_maxMessageLength == value) return;
            _maxMessageLength = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MaxMessageInfo));
        }
    }

    public string MaxMessageInfo => MaxMessageLength > 0
        ? $"Longueur max: {MaxMessageLength} octets"
        : "Longueur max: 0 (importez une image)";

    private int _messageByteCount;
    public int MessageByteCount
    {
        get => _messageByteCount;
        private set
        {
            if (_messageByteCount == value) return;
            _messageByteCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MessageUsageInfo));
        }
    }

    public string MessageUsageInfo => MaxMessageLength > 0
        ? $"Utilisé: {MessageByteCount}/{MaxMessageLength} octets"
        : $"Utilisé: {MessageByteCount}/0 octet";


 
    private async Task RecomputeMaxLengthFromCurrentImage()
    {
        this.MaxMessageLength = await this._encodeService.GetMaxInputLength(this.imagePath);
        EnforceUtf8Limit();
        UpdateMessageUsage();
    }

    private void OnEncodeDataTextChanged(object? sender, TextChangedEventArgs e)
    {
        EnforceUtf8Limit();
        UpdateMessageUsage();
    }

    private void EnforceUtf8Limit()
    {
        if (MaxMessageLength <= 0)
        {
            EncodeDataBox.Text = string.Empty;
            MessageByteCount = 0;
            return;
        }

        var text = EncodeDataBox.Text ?? string.Empty;
        var bytes = Encoding.UTF8.GetByteCount(text);
        if (bytes <= MaxMessageLength) return;

        EncodeDataBox.Text = TrimToUtf8ByteLength(text, MaxMessageLength);
        EncodeDataBox.CaretIndex = EncodeDataBox.Text?.Length ?? 0;
    }

    private void UpdateMessageUsage()
    {
        var currentText = EncodeDataBox.Text ?? string.Empty;
        MessageByteCount = Encoding.UTF8.GetByteCount(currentText);
    }

    // Trim to Utf-8, so Utf-16 is not considered and will be split at half
    private static string TrimToUtf8ByteLength(string input, int maxBytes)
    {
        if (string.IsNullOrEmpty(input) || maxBytes <= 0) return string.Empty;

        var sb = new StringBuilder(input.Length);
        int currentBytes = 0;

        foreach (char c in input)
        {
            int charBytes = Encoding.UTF8.GetByteCount(new[] { c });
            if (currentBytes + charBytes > maxBytes) break;
            sb.Append(c);
            currentBytes += charBytes;
        }

        return sb.ToString();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}

