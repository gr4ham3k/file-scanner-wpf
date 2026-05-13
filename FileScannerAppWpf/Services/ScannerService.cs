using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;
using System.Net.NetworkInformation;

namespace FileScannerApp.Services
{


    /// <summary>
    /// Obsluguje proces skanowania plikow pod katem zagrozen przy uzyciu zewnetrznego API VirusTotal.
    /// </summary>
    /// <remarks>
    /// Klasa laczy logike obliczania skrotu SHA-256, komunikacji z API, zapisu wynikow do bazy danych
    /// oraz raportowania postepu skanowania. Jest centralnym elementem funkcji bezpieczenstwa aplikacji.
    /// Ze wzgledu na ograniczenia API skanowanie wykonywane jest sekwencyjnie z opoznieniem miedzy plikami.
    /// </remarks>
    /// <seealso cref="Database"/>
    /// <seealso cref="ScanProgress"/>
    public class ScanService
    {
        private readonly string apiKey;
        private static readonly HttpClient client = new HttpClient();
        private readonly Database db;

        /// <summary>
        /// Tworzy usluge skanowania z konfiguracja API oraz dostepem do bazy danych.
        /// </summary>
        /// <remarks>
        /// Klucz API jest pobierany z konfiguracji aplikacji, dlatego obiekt tej klasy powinien byc tworzony
        /// dopiero wtedy, gdy konfiguracja zostala juz wczytana.
        /// </remarks>
        /// <param name="appConfig">Konfiguracja zawierajaca klucz do uslugi VirusTotal.</param>
        /// <param name="db">Obiekt odpowiedzialny za zapis historii skanow i wynikow.</param>
        public ScanService(AppConfig appConfig, Database db)
        {
            this.apiKey = appConfig.VirusTotalApiKey;
            this.db = db;
        }

        /// <summary>
        /// Rozpoczyna nowy wpis skanowania w bazie danych.
        /// </summary>
        /// <remarks>
        /// Metoda zapisuje skan ze statusem poczatkowym, aby pozniejsze wyniki plikow mogly byc
        /// przypisane do jednego procesu skanowania.
        /// </remarks>
        /// <param name="folder">Sciezka folderu wybranego do skanowania.</param>
        /// <param name="count">Liczba plikow przewidzianych do sprawdzenia.</param>
        /// <returns>Identyfikator utworzonego skanu, uzywany przy zapisie wynikow.</returns>
        public int CreateScan(string folder, int count)
        {
            return db.CreateScan(folder, count);
        }

        /// <summary>
        /// Skanuje kolekcje plikow, zapisuje wyniki i przekazuje postep do interfejsu uzytkownika.
        /// </summary>
        /// <remarks>
        /// Dla kazdego istniejacego pliku obliczany jest skrot SHA-256, a nastepnie pobierany jest raport
        /// z VirusTotal. Bledy pojedynczych plikow sa zapisywane jako wynik skanowania, aby awaria jednego
        /// pliku nie przerywala calego procesu. Po kazdym pliku wywolywany jest callback postepu.
        /// </remarks>
        /// <param name="files">Lista plikow wybranych do skanowania.</param>
        /// <param name="scanId">Identyfikator skanu, do ktorego zostana przypisane wyniki.</param>
        /// <param name="onProgress">Funkcja wywolywana po przetworzeniu kolejnego pliku.</param>
        /// <returns>Liczba plikow oznaczonych jako potencjalnie zlosliwe.</returns>
        public async Task<int> ScanFilesAsync(List<FileData> files, int scanId, Func<ScanProgress, Task> onProgress)
        {

            int threatsFound = 0;

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                string status = "Completed";
                string json = "";

                try
                {
                    if (!File.Exists(file.Path))
                        continue;

                    string hash = CalculateSHA256(file.Path);
                    json = await GetFileReportAsync(hash);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var doc = JsonDocument.Parse(json);

                        int malicious = doc.RootElement
                            .GetProperty("data")
                            .GetProperty("attributes")
                            .GetProperty("last_analysis_stats")
                            .GetProperty("malicious")
                            .GetInt32();

                        if (malicious > 0)
                        {
                            status = "Malicious";
                            threatsFound++;
                        }
                    }

                    db.SaveScanResult(scanId, file.Name, status, json);
                }
                catch (Exception ex)
                {
                    db.SaveScanResult(scanId, file.Name, "Error", ex.Message);
                }

                await onProgress(new ScanProgress
                {
                    Current = i + 1,
                    Total = files.Count,
                    CurrentFile = file.Name,
                    ThreatsFound = threatsFound
                });

                await Task.Delay(15000);
            }

            db.UpdateScanResults(scanId, threatsFound, "Completed");

            return threatsFound;
        }

        /// <summary>
        /// Oblicza skrot SHA-256 pliku, ktory jest uzywany jako identyfikator w usludze VirusTotal.
        /// </summary>
        /// <remarks>
        /// API VirusTotal pozwala sprawdzic plik po jego skrocie, bez wysylania zawartosci pliku.
        /// Metoda wymaga istniejacej sciezki, poniewaz otwiera plik bezposrednio z dysku.
        /// </remarks>
        /// <param name="filePath">Pelna sciezka do pliku, dla ktorego ma zostac obliczony skrot.</param>
        /// <returns>Skrot SHA-256 zapisany malymi literami bez separatorow.</returns>
        public string CalculateSHA256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }


        /// <summary>
        /// Pobiera raport VirusTotal dla pliku zidentyfikowanego przez skrot.
        /// </summary>
        /// <remarks>
        /// Gdy VirusTotal nie zna danego skrotu, metoda zwraca null. Inne bledy HTTP sa przekazywane dalej,
        /// poniewaz moga oznaczac problem z kluczem API, limitem zapytan albo dostepnoscia uslugi.
        /// </remarks>
        /// <param name="fileHash">Skrot SHA-256 pliku sprawdzanego w VirusTotal.</param>
        /// <returns>Odpowiedz API w formacie JSON albo null, gdy raport nie istnieje.</returns>
        public async Task<string> GetFileReportAsync(string fileHash)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://www.virustotal.com/api/v3/files/{fileHash}"),
                Headers =
                {
                    { "x-apikey", apiKey },
                    { "accept", "application/json" }
                }
            };

            using (var response = await client.SendAsync(request))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Sprawdza, czy aplikacja ma podstawowy dostep do internetu.
        /// </summary>
        /// <remarks>
        /// Metoda wykonuje krotkie zapytanie testowe i zwraca false przy dowolnym bledzie, aby interfejs
        /// mogl bezpiecznie poinformowac uzytkownika o braku polaczenia bez przerywania aplikacji wyjatkiem.
        /// </remarks>
        /// <returns>True, gdy testowe zapytanie HTTP zakonczylo sie powodzeniem; w przeciwnym razie false.</returns>
        public static async Task<bool> HasInternet()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);

                var response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
