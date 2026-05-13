using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileScannerApp
{
    /// <summary>
    /// Organizuje pliki w folderze docelowym wedlug regul wybranych przez uzytkownika.
    /// </summary>
    /// <remarks>
    /// Klasa pozwala kopiowac lub przenosic pliki, filtrowac je po rozszerzeniach oraz opcjonalnie
    /// tworzyc podfoldery odpowiadajace kategoriom plikow. Uwzglednia tez konflikty nazw,
    /// dzieki czemu istniejace pliki nie sa przypadkowo nadpisywane bez decyzji uzytkownika.
    /// </remarks>
    /// <seealso cref="FileData"/>
    /// <seealso cref="RenamePreview"/>
    public static class Organizer
    {
        /// <summary>
        /// Przenosi albo kopiuje pliki do folderu docelowego zgodnie z wybranymi filtrami i opcjami.
        /// </summary>
        /// <remarks>
        /// Metoda laczy kilka decyzji uzytkownika: wybrane typy plikow, tryb kopiowania lub przenoszenia,
        /// tworzenie podfolderow oraz podglad nowych nazw. Gdy plik docelowy juz istnieje, konflikt jest
        /// rozwiazywany przez nadpisanie albo dopisanie licznika do nazwy.
        /// </remarks>
        /// <param name="files">Pliki dostepne do organizowania.</param>
        /// <param name="sourceFolder">Folder zrodlowy, ktory musi istniec przed rozpoczeciem operacji.</param>
        /// <param name="destinationFolder">Folder docelowy; zostanie utworzony, jesli nie istnieje.</param>
        /// <param name="fileTypes">Lista rozszerzen dopuszczonych do organizowania; pusta lista oznacza brak filtrowania.</param>
        /// <param name="operation">Tryb operacji: "move" oznacza przeniesienie, pozostale wartosci powoduja kopiowanie.</param>
        /// <param name="createSubfolders">Okresla, czy pliki maja zostac rozdzielone do podfolderow wedlug typu.</param>
        /// <param name="overwriteExisting">Okresla, czy istniejace pliki w folderze docelowym moga zostac nadpisane.</param>
        /// <param name="db">Baza danych uzywana do zapisu historii operacji; moze byc null.</param>
        /// <param name="previews">Podglad nowych nazw przygotowany przed wykonaniem operacji.</param>
        /// <returns>Sciezka folderu docelowego, do ktorego organizowano pliki.</returns>
        /// <exception cref="DirectoryNotFoundException">Wyrzucany, gdy folder zrodlowy nie istnieje.</exception>
        public static string OrganizeFiles(
            List<FileData> files,
            string sourceFolder,
            string destinationFolder,
            List<string> fileTypes,
            string operation,
            bool createSubfolders,
            bool overwriteExisting,
            Database db,
            List<RenamePreview> previews)
        {
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException("Folder źródłowy nie istnieje.");

            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            foreach (var file in files)
            {
                if (!File.Exists(file.Path))
                    continue;

                string extension = file.Extension.ToLowerInvariant();

                if (fileTypes != null && fileTypes.Count > 0 && !fileTypes.Contains(extension))
                    continue;

                string targetFolder = destinationFolder;

                if (createSubfolders)
                {
                    string typeFolder = GetTypeFolder(extension);
                    targetFolder = Path.Combine(destinationFolder, typeFolder);

                    if (!Directory.Exists(targetFolder))
                        Directory.CreateDirectory(targetFolder);
                }

                string fileName = Path.GetFileNameWithoutExtension(file.Name);

                var preview = previews.FirstOrDefault(p => p.FullPath == file.Path);

                if (preview != null && !string.IsNullOrWhiteSpace(preview.NameAfter))
                {
                    fileName = Path.GetFileNameWithoutExtension(preview.NameAfter);
                }

                string fileExt = file.Extension;

                string targetPath = Path.Combine(targetFolder, fileName + fileExt);

                targetPath = ResolveConflict(fileName, fileExt, targetPath, overwriteExisting, targetFolder);

                try
                {
                    if (operation == "move")
                    {
                        File.Move(file.Path, targetPath);

                        db?.AddOperationLog(new OperationLog
                        {
                            OperationType = OperationType.Move,
                            FileName = Path.GetFileName(targetPath),
                            OldPath = file.Path,
                            NewPath = targetPath,
                            OperationDate = DateTime.Now,
                            CanUndo = true
                        });
                    }
                    else
                    {
                        File.Copy(file.Path, targetPath, overwriteExisting);

                        db?.AddOperationLog(new OperationLog
                        {
                            OperationType = OperationType.Copy,
                            FileName = Path.GetFileName(targetPath),
                            OldPath = file.Path,
                            NewPath = targetPath,
                            OperationDate = DateTime.Now,
                            CanUndo = false
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return destinationFolder;
        }

        /// <summary>
        /// Wyznacza bezpieczna sciezke docelowa, gdy w folderze istnieje juz plik o tej samej nazwie.
        /// </summary>
        /// <remarks>
        /// Jesli nadpisywanie jest wylaczone, metoda dodaje licznik w nawiasie do nazwy pliku
        /// i szuka pierwszej wolnej sciezki.
        /// </remarks>
        /// <param name="fileName">Nazwa pliku bez rozszerzenia.</param>
        /// <param name="fileExt">Rozszerzenie pliku razem z kropka.</param>
        /// <param name="targetPath">Pierwotnie planowana sciezka docelowa.</param>
        /// <param name="overwriteExisting">Informacja, czy istniejacy plik moze zostac nadpisany.</param>
        /// <param name="targetFolder">Folder, w ktorym szukana jest wolna nazwa.</param>
        /// <returns>Sciezka bez konfliktu albo pierwotna sciezka, gdy nadpisywanie jest dozwolone.</returns>
        private static string ResolveConflict(string fileName, string fileExt, string targetPath,
            bool overwriteExisting, string targetFolder)
        {
            if (!File.Exists(targetPath))
                return targetPath;

            if (overwriteExisting)
                return targetPath;

            int counter = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(targetFolder, $"{fileName}({counter}){fileExt}");
                counter++;
            }
            while (File.Exists(newPath));

            return newPath;
        }

        /// <summary>
        /// Dopasowuje rozszerzenie pliku do kategorii uzywanej przy tworzeniu podfolderow.
        /// </summary>
        /// <remarks>
        /// Kategorie sa pobierane z katalogu typow plikow. Nieznane rozszerzenia trafiaja do folderu "Others",
        /// aby organizowanie nadal dzialalo dla formatow spoza listy.
        /// </remarks>
        /// <param name="ext">Rozszerzenie pliku, na przyklad ".pdf".</param>
        /// <returns>Nazwa kategorii folderu dla danego rozszerzenia.</returns>
        private static string GetTypeFolder(string ext)
        {
            foreach (var group in FileTypeCatalog.Groups)
            {
                if (group.Value.Contains(ext))
                    return group.Key;
            }

            return "Others";
        }
    }
}
