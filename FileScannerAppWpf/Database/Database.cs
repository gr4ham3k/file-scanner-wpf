using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace FileScannerApp
{
    /// <summary>
    /// Zarzadza trwalym zapisem danych aplikacji w lokalnej bazie SQLite.
    /// </summary>
    /// <remarks>
    /// Klasa odpowiada za przechowywanie historii skanow, wynikow analizy plikow oraz dziennika operacji
    /// wykonywanych na plikach. Stanowi warstwe dostepu do danych dla uslug skanowania, cofania operacji
    /// i historii dzialan uzytkownika.
    /// </remarks>
    /// <seealso cref="Scan"/>
    /// <seealso cref="ScanResult"/>
    /// <seealso cref="OperationLog"/>
    public class Database
    {
        private string dbPath;

        /// <summary>
        /// Ustawia sciezke do pliku bazy danych uzywanej przez aplikacje.
        /// </summary>
        /// <remarks>
        /// Nazwa pliku jest laczona z katalogiem Data w folderze uruchomieniowym aplikacji.
        /// Dzieki temu kod korzysta z tej samej lokalizacji niezaleznie od miejsca instalacji programu.
        /// </remarks>
        /// <param name="dbPath">Nazwa pliku bazy danych; domyslnie "database.db".</param>
        public Database(string dbPath = "database.db")
        {
            this.dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                dbPath
            );
        }

        /// <summary>
        /// Otwiera polaczenie z baza, aby upewnic sie, ze plik bazy jest dostepny.
        /// </summary>
        /// <remarks>
        /// Metoda pelni role pomocnicza. Nie tworzy schematu tabel, dlatego zaklada,
        /// ze plik bazy i struktura danych sa juz przygotowane przez projekt.
        /// </remarks>
        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();
                connection.Close();
            }
        }

        /// <summary>
        /// Tworzy rekord nowego skanu i oznacza go jako trwajacy.
        /// </summary>
        /// <remarks>
        /// Rekord skanu jest zapisywany przed rozpoczeciem analizy pojedynczych plikow,
        /// aby wyniki mogly zostac powiazane z jedna sesja skanowania.
        /// </remarks>
        /// <param name="folderPath">Folder, ktory uzytkownik wybral do skanowania.</param>
        /// <param name="filesCount">Liczba plikow planowanych do sprawdzenia.</param>
        /// <returns>Identyfikator utworzonego rekordu skanu.</returns>
        public int CreateScan(string folderPath, int filesCount)
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                    INSERT INTO Scans (ScanDate, ScanPath, FilesCount, ThreatsFound, Status)
                    VALUES (@date, @path, @count, 0, 'InProgress');
                    SELECT last_insert_rowid();
                    ", connection);

                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString());
                cmd.Parameters.AddWithValue("@path", folderPath);
                cmd.Parameters.AddWithValue("@count", filesCount);

                long scanId = (long)cmd.ExecuteScalar();
                return (int)scanId;
            }
        }

        /// <summary>
        /// Zapisuje wynik skanowania pojedynczego pliku.
        /// </summary>
        /// <remarks>
        /// W bazie przechowywany jest zarowno uproszczony status, jak i surowa odpowiedz API.
        /// Surowy JSON pozwala pozniej odczytac dodatkowe informacje, na przyklad liczbe silnikow
        /// oznaczajacych plik jako zlosliwy.
        /// </remarks>
        /// <param name="scanId">Identyfikator skanu, do ktorego nalezy wynik.</param>
        /// <param name="fileName">Nazwa analizowanego pliku.</param>
        /// <param name="status">Status ustalony przez aplikacje, na przyklad Completed, Malicious albo Error.</param>
        /// <param name="apiResponse">Odpowiedz API VirusTotal albo opis bledu zapisany dla danego pliku.</param>
        public void SaveScanResult(int scanId, string fileName, string status, string apiResponse)
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                INSERT INTO ScanResults (ScanId, FileName, Status, ApiResponse)
                VALUES (@scanId, @fileName, @status, @response);
                ", connection);

                cmd.Parameters.AddWithValue("@scanId", scanId);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@response", apiResponse);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Aktualizuje podsumowanie skanu po zakonczeniu analizowania plikow.
        /// </summary>
        /// <remarks>
        /// Metoda zapisuje laczna liczbe wykrytych zagrozen i koncowy status skanu,
        /// dzieki czemu historia moze pokazac wynik bez ponownego przeliczania wszystkich plikow.
        /// </remarks>
        /// <param name="scanId">Identyfikator skanu do zaktualizowania.</param>
        /// <param name="threatsFound">Liczba plikow oznaczonych jako zagrozenie.</param>
        /// <param name="status">Koncowy status skanu.</param>
        public void UpdateScanResults(int scanId, int threatsFound, string status)
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                    UPDATE Scans
                    SET ThreatsFound = @threats, Status = @status
                    WHERE Id = @scanId;
                ", connection);

                cmd.Parameters.AddWithValue("@threats", threatsFound);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@scanId", scanId);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Pobiera pliki nalezace do konkretnego skanu wraz z uproszczona informacja o zagrozeniu.
        /// </summary>
        /// <remarks>
        /// Metoda interpretuje zapisany JSON z VirusTotal i sprowadza wynik do wartosci czytelnej dla widoku.
        /// Jesli odpowiedz API jest pusta albo nie da sie jej odczytac, plik jest traktowany jako niezlosliwy
        /// na potrzeby prezentacji.
        /// </remarks>
        /// <param name="scanId">Identyfikator skanu, ktorego wyniki maja zostac pobrane.</param>
        /// <returns>Lista obiektow zawierajacych nazwe pliku, status i informacje Malicious.</returns>
        public List<dynamic> GetFilesForScan(int scanId)
        {
            var files = new List<dynamic>();

            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                SELECT FileName, Status, ApiResponse
                FROM ScanResults
                WHERE ScanId = @scanId
                ORDER BY FileName;
                ", connection);

                cmd.Parameters.AddWithValue("@scanId", scanId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int malicious = 0;
                        string json = reader.IsDBNull(2) ? null : reader.GetString(2);

                        if (!string.IsNullOrEmpty(json))
                        {
                            try
                            {
                                var doc = JsonDocument.Parse(json);
                                malicious = doc.RootElement
                                               .GetProperty("data")
                                               .GetProperty("attributes")
                                               .GetProperty("last_analysis_stats")
                                               .GetProperty("malicious")
                                               .GetInt32();
                            }
                            catch
                            {
                                malicious = 0;
                            }
                        }

                        files.Add(new
                        {
                            FileName = reader.GetString(0),
                            Status = reader.GetString(1),
                            Malicious = malicious > 0 ? "Yes" : "No"
                        });
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Pobiera historie skanow razem z powiazanymi plikami.
        /// </summary>
        /// <remarks>
        /// Wynik laczy dane tabeli skanow i wynikow plikow, aby okno historii moglo pokazac pelny obraz
        /// poprzednich analiz. Brak wyniku pliku nie usuwa skanu z listy, poniewaz uzywane jest zlaczenie LEFT JOIN.
        /// </remarks>
        /// <returns>Lista dynamicznych rekordow przeznaczonych do wyswietlenia w historii skanowania.</returns>
        public List<dynamic> GetAllScansWithFiles()
        {
            var scans = new List<dynamic>();

            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                SELECT s.Id, s.ScanDate, s.ScanPath, s.FilesCount, s.ThreatsFound, s.Status,
                   r.FileName, r.Status AS FileStatus, r.ApiResponse
                FROM Scans s
                LEFT JOIN ScanResults r ON s.Id = r.ScanId
                ORDER BY s.Id DESC, r.FileName ASC;
                ", connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int malicious = 0;
                        string json = reader.IsDBNull(8) ? null : reader.GetString(8);

                        if (!string.IsNullOrEmpty(json))
                        {
                            try
                            {
                                var doc = JsonDocument.Parse(json);
                                malicious = doc.RootElement
                                               .GetProperty("data")
                                               .GetProperty("attributes")
                                               .GetProperty("last_analysis_stats")
                                               .GetProperty("malicious")
                                               .GetInt32();
                            }
                            catch
                            {
                                malicious = 0;
                            }
                        }

                        scans.Add(new
                        {
                            ScanId = reader.GetInt32(0),
                            ScanDate = reader.GetString(1),
                            ScanPath = reader.GetString(2),
                            FilesCount = reader.GetInt32(3),
                            ThreatsFound = reader.GetInt32(4),
                            ScanStatus = reader.GetString(5),
                            FileName = reader.IsDBNull(6) ? null : reader.GetString(6),
                            FileStatus = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Malicious = malicious > 0 ? "Yes" : "No"
                        });
                    }
                }
            }

            return scans;
        }

        /// <summary>
        /// Dodaje wpis do historii operacji wykonanych na plikach.
        /// </summary>
        /// <remarks>
        /// Historia przechowuje sciezki sprzed i po operacji, poniewaz sa one potrzebne do cofania zmian
        /// oraz do pokazania uzytkownikowi, co dokladnie stalo sie z plikiem.
        /// </remarks>
        /// <param name="log">Opis operacji, ktora ma zostac zapisana.</param>
        public void AddOperationLog(OperationLog log)
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                    INSERT INTO OperationsLog
                    (OperationType, FileName, OldPath, NewPath, OperationDate, CanUndo)
                    VALUES
                    (@type, @fileName, @oldPath, @newPath, @date, @canUndo);
                ", connection);

                cmd.Parameters.AddWithValue("@type", log.OperationType.ToString());
                cmd.Parameters.AddWithValue("@fileName", log.FileName);

                cmd.Parameters.AddWithValue("@oldPath",
                    (object)log.OldPath ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@newPath",
                    (object)log.NewPath ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@date",
                    log.OperationDate.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.Parameters.AddWithValue("@canUndo",
                    log.CanUndo ? 1 : 0);

                cmd.ExecuteNonQuery();
            }
        }




        /// <summary>
        /// Pobiera historie operacji plikowych od najnowszych wpisow.
        /// </summary>
        /// <remarks>
        /// Dane z bazy sa mapowane z powrotem na model <see cref="OperationLog"/>, w tym na typ wyliczeniowy
        /// operacji. Kolejnosc sortowania ulatwia pokazanie ostatnich dzialan na gorze listy.
        /// </remarks>
        /// <returns>Lista wpisow historii operacji.</returns>
        public List<OperationLog> GetOperationsLog()
        {
            var logs = new List<OperationLog>();

            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                string query = @"
                    SELECT Id, OperationType, FileName, OldPath, NewPath, OperationDate, CanUndo
                    FROM OperationsLog
                    ORDER BY OperationDate DESC, Id DESC;
                ";

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new OperationLog
                        {
                            Id = reader.GetInt32(0),

                            OperationType = (OperationType)Enum.Parse(typeof(OperationType), reader.GetString(1)),

                            FileName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            OldPath = reader.IsDBNull(3) ? null : reader.GetString(3),
                            NewPath = reader.IsDBNull(4) ? null : reader.GetString(4),

                            OperationDate = DateTime.Parse(reader.GetString(5)),

                            CanUndo = reader.GetInt32(6) == 1
                        });
                    }
                }
            }

            return logs;
        }

        /// <summary>
        /// Usuwa pojedynczy wpis historii operacji.
        /// </summary>
        /// <remarks>
        /// Metoda usuwa tylko rekord historii. Nie wykonuje zadnej operacji na pliku,
        /// dlatego nie nalezy jej traktowac jako cofniecia lub usuniecia pliku z dysku.
        /// </remarks>
        /// <param name="id">Identyfikator wpisu historii do usuniecia.</param>
        public void DeleteOperationLog(int id)
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                    DELETE FROM OperationsLog
                    WHERE Id = @id;
                ", connection);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Oznacza plik z kosza aplikacji jako trwale usuniety.
        /// </summary>
        /// <remarks>
        /// Po wyczyszczeniu kosza aplikacja nie powinna juz oferowac cofniecia operacji.
        /// Metoda aktualizuje wpis historii, ustawiajac typ operacji na trwale usuniecie i blokujac cofanie.
        /// </remarks>
        /// <param name="path">Sciezka pliku w wewnetrznym koszu aplikacji.</param>
        public void MarkAsDeletedPermanently(string path)
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();

                var cmd = new SQLiteCommand(@"
                    UPDATE OperationsLog
                    SET OperationType = @type,
                    CanUndo = 0
                    WHERE NewPath = @path;
                ", connection);

                cmd.Parameters.AddWithValue("@type", OperationType.DeletedPermanently.ToString());
                cmd.Parameters.AddWithValue("@path", path);

                cmd.ExecuteNonQuery();
            }
        }

    }
}
