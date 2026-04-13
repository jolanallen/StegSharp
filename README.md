# SteganoApp

A modern steganography application for hiding and extracting secret messages within digital media files.

## Overview

SteganoApp provides a straightforward interface to embed confidential information into images and retrieve hidden messages without detection. Built with C# and .NET, the application ensures secure data concealment with minimal file quality degradation.

## Features

- Hide text messages in image files
- Extract embedded messages from carrier images
- Support for multiple image formats (PNG, BMP, JPEG)
- Minimal visual quality loss
- Cross-platform compatibility (Windows, Linux, macOS)
- Command-line and GUI interfaces

## Requirements

- .NET 10.0 or later
- Windows, Linux, or macOS

## Installation

### From Source

```bash
git clone https://github.com/yourusername/SteganoApp.git
cd SteganoApp
dotnet restore SteganoApp.slnx
dotnet build SteganoApp.slnx --configuration Release
```

### From Release

Download the latest pre-built binary from the [Releases page](https://github.com/yourusername/SteganoApp/releases).

## Usage

### Command Line

```bash
# Hide a message
stegano encode --image input.png --message "Secret text" --output output.png

# Extract a message
stegano decode --image output.png --output message.txt
```

### API

```csharp
using Stegano.Core;

var encoder = new SteganographyEncoder();
encoder.HideMessage("input.png", "My secret", "output.png");

var decoder = new SteganographyDecoder();
var result = decoder.ExtractMessage("output.png");

if (result.Succeeded)
    Console.WriteLine(result.Message);
```

## Architecture

- **Stegano.Core** - Core steganography algorithms
- **Stegano.CLI** - Command-line interface
- **Stegano.UI** - Graphical user interface (WPF/WinForms)
- **Stegano.Tests** - Unit and integration tests

## Technical Details

The application uses LSB (Least Significant Bit) steganography to embed data within image pixels, ensuring imperceptible changes to the carrier image.

## Building and Testing

```bash
# Build release
dotnet build SteganoApp.slnx --configuration Release

# Run tests
dotnet test SteganoApp.slnx --configuration Release

# Publish for specific platform
dotnet publish SteganoApp.slnx --configuration Release -r win-x64 -o ./publish
```

## CI/CD

Automated builds and releases are configured via GitHub Actions:
- **build.yml** - Runs on every push and pull request
- **Release.yml** - Creates release artifacts for tagged versions (v*.*.*)

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome. Please ensure all tests pass before submitting pull requests.

## Support

For issues and feature requests, please use the GitHub Issues page.
