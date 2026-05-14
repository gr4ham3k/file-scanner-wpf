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
    /// Obsługuje proces skanowania plikow pod kątem zagrożen przy użyciu zewnętrznego API VirusTotal.
    /// </summary>
    /// <remarks>
    /// Klasa łączy logikę obliczania skrótu SHA-256, komunikacji z API, zapisu wyników do bazy danych
    /// oraz raportowania postępu skanowania.
    /// </remarks>
    /// <seealso cref="Database"/>
    /// <seealso cref="ScanProgress"/>
    public class ScanService
    {
        private readonly string apiKey;
        private static readonly HttpClient client = new HttpClient();
        private readonly Database db;

        /// <summary>
        /// Tworzy usługę skanowania z konfiguracją API oraz dostępem do bazy danych.
        /// </summary>
        /// <remarks>
        /// Klucz API jest pobierany z konfiguracji aplikacji.
        /// </remarks>
        /// <param name="appConfig">Konfiguracja zawierająca klucz do usługi VirusTotal.</param>
        /// <param name="db">Obiekt odpowiedzialny za zapis historii skanów i wyników.</param>
        public ScanService(AppConfig appConfig, Database db)
        {
            this.apiKey = appConfig.VirusTotalApiKey;
            this.db = db;
        }

        /// <summary>
        /// Rozpoczyna nowy wpis skanowania w bazie danych.
        /// </summary>
        /// <remarks>
        /// Metoda zapisuje skan ze statusem początkowym, aby późniejsze wyniki plików mogły być
        /// przypisane do jednego procesu skanowania.
        /// </remarks>
        /// <param name="folder">Scieżka folderu wybranego do skanowania.</param>
        /// <param name="count">Liczba plików przewidzianych do sprawdzenia.</param>
        /// <returns>Identyfikator utworzonego skanu, używany przy zapisie wyników.</returns>
        public int CreateScan(string folder, int count)
        {
            return db.CreateScan(folder, count);
        }

        /// <summary>
        /// Skanuje kolekcje plików, zapisuje wyniki i przekazuje postęp do interfejsu użytkownika.
        /// </summary>
        /// <remarks>
        /// Dla każdego istniejącego pliku obliczany jest skrót SHA-256, a następnie pobierany jest raport
        /// z VirusTotal. Błędy pojedyńczych plików są zapisywane jako wynik skanowania, aby awaria jednego
        /// pliku nie przerywala całego procesu. Po każdym pliku wywoływany jest callback postępu.
        /// </remarks>
        /// <param name="files">Lista plików wybranych do skanowania.</param>
        /// <param name="scanId">Identyfikator skanu, do którego zostaną przypisane wyniki.</param>
        /// <param name="onProgress">Funkcja wywoływana po przetworzeniu kolejnego pliku.</param>
        /// <returns>Liczba plików oznaczonych jako potencjalnie złosliwe.</returns>
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
        /// Oblicza skrót SHA-256 pliku, który jest używany jako identyfikator w usłudze VirusTotal.
        /// </summary>
        /// <remarks>
        /// API VirusTotal pozwala sprawdzić plik po jego skrócie, bez wysyłania zawartosci pliku.
        /// Metoda wymaga istniejącej ścieżki, ponieważ otwiera plik bezpośrednio z dysku.
        /// </remarks>
        /// <param name="filePath">Pełna ścieżka do pliku, dla ktorego ma zostać obliczony skrót.</param>
        /// <returns>Skrót SHA-256 zapisany małymi literami bez separatorow.</returns>
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
        /// Pobiera raport VirusTotal dla pliku zidentyfikowanego przez skrót.
        /// </summary>
        /// <remarks>
        /// Gdy VirusTotal nie zna danego skrótu, metoda zwraca null. Inne błędy HTTP są przekazywane dalej,
        /// ponieważ mogą oznaczać problem z kluczem API, limitem zapytań albo dostępnoscią usługi.
        /// </remarks>
        /// <param name="fileHash">Skrót SHA-256 pliku sprawdzanego w VirusTotal.</param>
        /// <returns>Odpowiedź API w formacie JSON albo null, gdy raport nie istnieje.</returns>
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
        /// Sprawdza, czy aplikacja ma podstawowy dostęp do internetu.
        /// </summary>
        /// <remarks>
        /// Metoda wykonuje krótkie zapytanie testowe i zwraca false przy dowolnym błędzie, aby interfejs
        /// mógl bezpiecznie poinformowac użytkownika o braku połączenia bez przerywania aplikacji wyjątkiem.
        /// </remarks>
        /// <returns>True, gdy testowe zapytanie HTTP zakończyło sie powodzeniem; w przeciwnym razie false.</returns>
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
