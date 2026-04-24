using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;

namespace FileScannerApp.Services
{


    public class ScanService
    {
        private readonly string apiKey;
        private static readonly HttpClient client = new HttpClient();
        private readonly Database db;

        public ScanService(AppConfig appConfig, Database db)
        {
            this.apiKey = appConfig.VirusTotalApiKey;
            this.db = db;
        }

        public int CreateScan(string folder, int count)
        {
            return db.CreateScan(folder, count);
        }

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
    }
}