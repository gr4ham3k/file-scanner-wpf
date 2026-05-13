using FileScannerApp.Models;
using FileScannerApp.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileScannerApp.Services
{
    /// <summary>
    /// Wykonuje podstawowe operacje na plikach wybrane przez uzytkownika.
    /// </summary>
    /// <remarks>
    /// Usluga obsluguje przenoszenie, zmiane nazw, usuwanie do wewnetrznego kosza aplikacji
    /// oraz przywracanie plikow. Operacje, ktore mozna pozniej przeanalizowac lub cofnac,
    /// sa zapisywane w historii.
    /// </remarks>
    /// <seealso cref="OperationLog"/>
    /// <seealso cref="OperationType"/>
    /// <seealso cref="Database"/>
    public class FileOperationsService
    {
        private readonly Database db;

        /// <summary>
        /// Tworzy usluge operacji plikowych powiazana z historia operacji w bazie danych.
        /// </summary>
        /// <param name="db">Baza danych uzywana do zapisywania operacji wykonanych na plikach.</param>
        public FileOperationsService(Database db)
        {
            this.db = db;
        }

        /// <summary>
        /// Przenosi wskazane pliki do wewnetrznego kosza aplikacji zamiast usuwac je trwale.
        /// </summary>
        /// <remarks>
        /// Kazdy plik otrzymuje unikalna nazwe w katalogu kosza, co chroni przed konfliktami nazw.
        /// Udane przeniesienie jest zapisywane jako operacja mozliwa do cofniecia.
        /// </remarks>
        /// <param name="paths">Sciezki plikow przeznaczonych do usuniecia z widoku uzytkownika.</param>
        /// <returns>Liczba plikow przeniesionych do kosza oraz liczba operacji nieudanych.</returns>
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
        /// Przenosi pliki do wybranego folderu docelowego i zwraca zaktualizowane sciezki.
        /// </summary>
        /// <remarks>
        /// Metoda pomija pliki, ktore nie istnieja, oraz pliki powodujace konflikt nazwy w folderze docelowym.
        /// Udane przeniesienia sa zapisywane w historii jako operacje mozliwe do cofniecia.
        /// </remarks>
        /// <param name="paths">Lista sciezek plikow do przeniesienia.</param>
        /// <param name="targetFolder">Folder, do ktorego maja trafic pliki.</param>
        /// <returns>Liczba przeniesionych plikow, liczba pominietych plikow oraz lista zmian sciezek.</returns>
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
        /// Zmienia nazwe pojedynczego pliku i zapisuje operacje w historii.
        /// </summary>
        /// <remarks>
        /// Jesli nowa nazwa nie zawiera oryginalnego rozszerzenia, metoda dopisuje je automatycznie.
        /// Zamiast rzucac wyjatki przy typowych problemach uzytkownika, zwraca opis bledu w wyniku metody.
        /// </remarks>
        /// <param name="oldPath">Aktualna sciezka pliku.</param>
        /// <param name="newName">Nowa nazwa pliku podana przez uzytkownika.</param>
        /// <returns>Informacja o powodzeniu, nowa sciezka albo komunikat bledu.</returns>
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
        /// Trwale usuwa pliki znajdujace sie w wewnetrznym koszu aplikacji.
        /// </summary>
        /// <remarks>
        /// Po usunieciu pliku odpowiadajacy wpis historii jest oznaczany jako trwale usuniety,
        /// aby aplikacja nie probowala pozniej przywrocic pliku, ktorego juz nie ma.
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
        /// Metoda korzysta ze sciezki docelowej zapisanej przy usunieciu lub przeniesieniu pliku.
        /// Jesli plik nie istnieje albo nie da sie ustalic folderu docelowego, metoda konczy prace bez bledu.
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
