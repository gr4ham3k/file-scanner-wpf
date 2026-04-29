using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace FileScannerApp
{
    public class Database
    {
        private string dbPath;

        public Database(string dbPath = "database.db")
        {
            this.dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                dbPath
            );
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection($"Data Source={this.dbPath}"))
            {
                connection.Open();
                connection.Close();
            }
        }

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
