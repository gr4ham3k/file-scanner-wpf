using PdfiumViewer;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using MediaColor = System.Windows.Media.Color;
using TextBox = System.Windows.Controls.TextBox;

namespace FileScannerApp.Wpf.Helpers;

public static class PreviewFactory
{
    public static FrameworkElement Create(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return CreateMessage("Select a file to preview.");
        }

        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        if (new[] { ".jpg", ".png", ".bmp", ".gif" }.Contains(extension))
        {
            var image = new Image
            {
                Stretch = Stretch.Uniform,
                Source = new BitmapImage(new Uri(filePath))
            };

            return new ScrollViewer { Content = image };
        }

        if (extension == ".txt")
        {
            return new TextBox
            {
                Text = File.ReadAllText(filePath),
                IsReadOnly = true,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                TextWrapping = TextWrapping.Wrap
            };
        }

        if (new[] { ".mp3", ".wav", ".mp4", ".avi", ".mkv" }.Contains(extension))
        {
            var media = new MediaElement
            {
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
                Stretch = Stretch.Uniform,
                Source = new Uri(filePath)
            };

            media.Loaded += (_, _) => media.Play();
            return media;
        }

        if (extension == ".pdf")
        {
            var viewer = new PdfViewer
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Document = PdfDocument.Load(filePath)
            };

            return new WindowsFormsHost { Child = viewer };
        }

        return CreateMessage("Preview not supported for this file type.");
    }

    private static FrameworkElement CreateMessage(string message)
    {
        return new Grid
        {
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(MediaColor.FromRgb(90, 100, 114)),
                    FontSize = 16
                }
            }
        };
    }
}
