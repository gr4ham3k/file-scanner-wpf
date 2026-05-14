using FileScannerApp.Models;
using FileScannerApp.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileScannerApp.Services
{
    /// <summary>
    /// Wykonuje podstawowe operacje na plikach wybrane przez użytkownika.
    /// </summary>
    /// <remarks>
    /// Usługa obsługuje przenoszenie, zmianę nazw, usuwanie do wewnętrznego kosza aplikacji
    /// oraz przywracanie plikow. Operacje, które można później przeanalizowac lub cofnąć,
    /// są zapisywane w historii.
    /// </remarks>
    /// <seealso cref="OperationLog"/>
    /// <seealso cref="OperationType"/>
    /// <seealso cref="Database"/>
    public class FileOperationsService
    {
        private readonly Database db;

        /// <summary>
        /// Tworzy usługę operacji plikowych powiązaną z historią operacji w bazie danych.
        /// </summary>
        /// <param name="db">Baza danych używana do zapisywania operacji wykonanych na plikach.</param>
        public FileOperationsService(Database db)
        {
            this.db = db;
        }

        /// <summary>
        /// Przenosi wskazane pliki do wewnętrznego kosza aplikacji zamiast usuwać je trwale.
        /// </summary>
        /// <remarks>
        /// Każdy plik otrzymuje unikalną nazwę w katalogu kosza, co chroni przed konfliktami nazw.
        /// Udane przeniesienie jest zapisywane jako operacja możliwa do cofnięcia.
        /// </remarks>
        /// <param name="paths">Ścieżki plików przeznaczonych do usunięcia z widoku użytkownika.</param>
        /// <returns>Liczba plików przeniesionych do kosza oraz liczba operacji nieudanych.</returns>
        public (int moved, int failed) DeleteFiles(List<string> paths)
        {
            int movedCount = 0;
            int failedCount = 0;

            var binRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (!Directory.Exists(binRoot))
                Directory.CreateDirectory(binRoot);

            foreach (var path in paths)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        failedCount++;
                        continue;
                    }

                    string fileName = Path.GetFileName(path);
                    string uniqueName = $"{Guid.NewGuid()}_{fileName}";
                    string destPath = Path.Combine(binRoot, uniqueName);

                    File.Move(path, destPath);

                    db.AddOperationLog(new OperationLog
                    {
                        OperationType = OperationType.Delete,
                        FileName = fileName,
                        OldPath = path,
                        NewPath = destPath,
                        OperationDate = DateTime.Now,
                        CanUndo = true
                    });

                    movedCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            return (movedCount, failedCount);
        }

        /// <summary>
        /// Przenosi pliki do wybranego folderu docelowego i zwraca zaktualizowane ścieżki.
        /// </summary>
        /// <remarks>
        /// Metoda pomija pliki, ktore nie istnieją, oraz pliki powodujące konflikt nazwy w folderze docelowym.
        /// Udane przeniesienia są zapisywane w historii jako operacje możliwe do cofnięcia.
        /// </remarks>
        /// <param name="paths">Lista ścieżek plików do przeniesienia.</param>
        /// <param name="targetFolder">Folder, do którego mają trafić pliki.</param>
        /// <returns>Liczba przeniesionych plików, liczba pominiętych plików oraz lista zmian ścieżek.</returns>
        public (int moved, int skipped, List<(string oldPath, string newPath)> updatedPaths)
        MoveFiles(List<string> paths, string targetFolder)
        {

            int movedCount = 0;
            int skippedCount = 0;

            var updated = new List<(string, string)>();

            foreach (var oldPath in paths)
            {
                try
                {
                    if (!File.Exists(oldPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    string fileName = Path.GetFileName(oldPath);
                    string newPath = Path.Combine(targetFolder, fileName);

                    if (File.Exists(newPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    File.Move(oldPath, newPath);


                    db.AddOperationLog(new OperationLog
                    {
                        OperationType = OperationType.Move,
                        FileName = fileName,
                        OldPath = oldPath,
                        NewPath = newPath,
                        OperationDate = DateTime.Now,
                        CanUndo = true
                    });

                    updated.Add((oldPath, newPath));
                    movedCount++;
                }
                catch
                {
                    skippedCount++;
                }
            }

            return (movedCount, skippedCount, updated);
        }

        /// <summary>
        /// Zmienia nazwę pojedyńczego pliku i zapisuje operacje w historii.
        /// </summary>
        /// <remarks>
        /// Jeśli nowa nazwa nie zawiera oryginalnego rozszerzenia, metoda dopisuje je automatycznie.
        /// Zamiast rzucać wyjątki przy typowych problemach użytkownika, zwraca opis błędu w wyniku metody.
        /// </remarks>
        /// <param name="oldPath">Aktualna ścieżka pliku.</param>
        /// <param name="newName">Nowa nazwa pliku podana przez użytkownika.</param>
        /// <returns>Informacja o powodzeniu, nowa ścieżka albo komunikat bledu.</returns>
        public (bool success, string newPath, string error) RenameFile(string oldPath, string newName)
        {
            try
            {
                if (!File.Exists(oldPath))
                    return (false, null, "File does not exist.");

                string directory = Path.GetDirectoryName(oldPath);
                string extension = Path.GetExtension(oldPath);

                if (!newName.EndsWith(extension))
                    newName += extension;

                string newPath = Path.Combine(directory, newName);

                if (File.Exists(newPath))
                    return (false, null, "File with this name already exists.");

                File.Move(oldPath, newPath);

                db.AddOperationLog(new OperationLog
                {
                    OperationType = OperationType.Rename,
                    FileName = newName,
                    OldPath = oldPath,
                    NewPath = newPath,
                    OperationDate = DateTime.Now,
                    CanUndo = true
                });

                return (true, newPath, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Trwale usuwa pliki znajdujące się w wewnętrznym koszu aplikacji.
        /// </summary>
        /// <remarks>
        /// Po usunięciu pliku odpowiadający wpis historii jest oznaczany jako trwale usunięty,
        /// aby aplikacja nie próbowała później przywrócić pliku, którego już nie ma.
        /// </remarks>
        public void CleanupBin()
        {
            var binRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (!Directory.Exists(binRoot))
                return;

            foreach (var file in Directory.GetFiles(binRoot))
            {
                File.Delete(file);

                db.MarkAsDeletedPermanently(file);
            }
        }

        /// <summary>
        /// Przywraca plik na podstawie wpisu historii operacji.
        /// </summary>
        /// <remarks>
        /// Metoda korzysta ze ścieżki docelowej zapisanej przy usunięciu lub przeniesieniu pliku.
        /// Jesli plik nie istnieje albo nie da sie ustalić folderu docelowego, metoda kończy pracę bez błędu.
        /// </remarks>
        /// <param name="log">Wpis historii zawierajacy poprzednia i aktualna sciezke pliku.</param>
        public void RestoreFile(OperationLog log)
        {
            if (string.IsNullOrEmpty(log.NewPath) || !File.Exists(log.NewPath))
                return;

            string? targetDir = Path.GetDirectoryName(log.OldPath);

            if (string.IsNullOrEmpty(targetDir))
                return;

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            File.Move(log.NewPath, log.OldPath);

            db.AddOperationLog(new OperationLog
            {
                OperationType = OperationType.Move,
                FileName = log.FileName,
                OldPath = log.NewPath,
                NewPath = log.OldPath,
                OperationDate = DateTime.Now,
                CanUndo = false
            });
        }
    }
}
