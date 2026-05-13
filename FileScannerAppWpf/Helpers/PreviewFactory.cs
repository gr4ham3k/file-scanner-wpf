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
/// Tworzy element interfejsu sluzacy do podgladu zawartosci wybranego pliku.
/// </summary>
/// <remarks>
/// Fabryka dobiera sposob prezentacji pliku na podstawie jego rozszerzenia. Obsluguje miedzy innymi
/// obrazy, pliki tekstowe, multimedia, PDF oraz DOCX. Dla nieobslugiwanych formatow zwraca komunikat
/// zamiast powodowac blad interfejsu.
/// </remarks>
/// <seealso cref="ConvertDocxToHtml"/>
public static class PreviewFactory
{
    /// <summary>
    /// Tworzy kontrolke WPF odpowiednia do podgladu wskazanego pliku.
    /// </summary>
    /// <remarks>
    /// Metoda ukrywa szczegoly tworzenia roznych kontrolek podgladu przed reszta aplikacji.
    /// Dzieki temu okno glowne moze ustawic jeden element interfejsu niezaleznie od tego,
    /// czy uzytkownik wybral obraz, tekst, multimedia, dokument PDF czy plik DOCX.
    /// </remarks>
    /// <param name="filePath">Sciezka do pliku, ktory ma zostac wyswietlony w panelu podgladu.</param>
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
    /// Czysci panel podgladu i zatrzymuje odtwarzanie multimediow, jesli byly aktywne.
    /// </summary>
    /// <remarks>
    /// Samo usuniecie zawartosci nie zawsze wystarcza dla plikow audio lub wideo, dlatego metoda
    /// jawnie zatrzymuje i zamyka kontrolke multimedialna przed wyczyszczeniem panelu.
    /// </remarks>
    /// <param name="preview">Kontrolka zawierajaca aktualnie wyswietlany podglad.</param>
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
    /// Tworzy prosty komunikat wyswietlany w miejscu podgladu pliku.
    /// </summary>
    /// <remarks>
    /// Komunikat jest uzywany zamiast wyjatku, gdy nie wybrano pliku albo format nie jest obslugiwany.
    /// Dzieki temu panel podgladu pozostaje stabilny nawet dla nieznanych typow plikow.
    /// </remarks>
    /// <param name="message">Tekst komunikatu widoczny dla uzytkownika.</param>
    /// <returns>Element WPF z wysrodkowanym komunikatem.</returns>
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
