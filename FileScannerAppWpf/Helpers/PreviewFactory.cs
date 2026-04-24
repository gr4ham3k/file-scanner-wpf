using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using MediaColor = System.Windows.Media.Color;
using TextBox = System.Windows.Controls.TextBox;
using Microsoft.Web.WebView2.Wpf;


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
                Source = new Uri(filePath),
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop
            };

            media.Play();

            return media;
        }
        if (extension == ".pdf")
        {
            var web = new WebView2();
            web.Source = new Uri(filePath);

            return web;
        }

        if (extension == ".docx")
        {
            var web = new WebView2();

            web.Loaded += async (s, e) =>
            {
                await web.EnsureCoreWebView2Async();

                string html = await Task.Run(() =>
                    ConvertDocxToHtml.Convert(filePath));

                web.NavigateToString(html);
            };

            return web;
        }

        return CreateMessage("Preview not supported for this file type.");
    }

    public static void ClearPreview(ContentControl preview)
    {
        if (preview.Content is MediaElement media)
        {
            media.Stop();
            media.Close();
        }

        preview.Content = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
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
