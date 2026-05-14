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

/// <summary>
/// Tworzy element interfejsu służacy do podglądu zawartosci wybranego pliku.
/// </summary>
/// <remarks>
/// Fabryka dobiera sposób prezentacji pliku na podstawie jego rozszerzenia. Obsluguje między innymi
/// obrazy, pliki tekstowe, multimedia, PDF oraz DOCX. Dla nieobsługiwanych formatów zwraca komunikat
/// zamiast powodować błąd interfejsu.
/// </remarks>
/// <seealso cref="ConvertDocxToHtml"/>
public static class PreviewFactory
{
    /// <summary>
    /// Tworzy kontrolkę WPF odpowiednią do podglądu wskazanego pliku.
    /// </summary>
    /// <remarks>
    /// Metoda ukrywa szczegóły tworzenia różnych kontrolek podglądu przed resztą aplikacji.
    /// Dzięki temu okno główne może ustawić jeden element interfejsu niezależnie od tego,
    /// czy użytkownik wybrał obraz, tekst, multimedia, dokument PDF czy plik DOCX.
    /// </remarks>
    /// <param name="filePath">Ścieżka do pliku, który ma zostać wyćwietlony w panelu podglądu.</param>
    /// <returns>Gotowy element WPF do umieszczenia w kontrolce zawartosci.</returns>
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

    /// <summary>
    /// Czyści panel podglądu i zatrzymuje odtwarzanie multimediów, jeśli byly aktywne.
    /// </summary>
    /// <remarks>
    /// Samo usunięcie zawartości nie zawsze wystarcza dla plików audio lub wideo, dlatego metoda
    /// jawnie zatrzymuje i zamyka kontrolkę multimedialną przed wyczyszczeniem panelu.
    /// </remarks>
    /// <param name="preview">Kontrolka zawierająca aktualnie wyświetlany podgląd.</param>
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

    /// <summary>
    /// Tworzy prosty komunikat wyświetlany w miejscu podglądu pliku.
    /// </summary>
    /// <remarks>
    /// Komunikat jest używany zamiast wyjątku, gdy nie wybrano pliku albo format nie jest obsługiwany.
    /// Dzięki temu panel podglądu pozostaje stabilny nawet dla nieznanych typów plików.
    /// </remarks>
    /// <param name="message">Tekst komunikatu widoczny dla użytkownika.</param>
    /// <returns>Element WPF z wyśrodkowanym komunikatem.</returns>
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
