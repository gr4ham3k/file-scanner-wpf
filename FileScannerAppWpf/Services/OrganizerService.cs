using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileScannerApp
{
    /// <summary>
    /// Organizuje pliki w folderze docelowym według reguł wybranych przez użytkownika.
    /// </summary>
    /// <remarks>
    /// Klasa pozwala kopiować lub przenosić pliki, filtrować je po rozszerzeniach oraz opcjonalnie
    /// tworzyć podfoldery odpowiadające kategoriom plików. Uwzględnia też konflikty nazw,
    /// dzięki czemu istniejące pliki nie są przypadkowo nadpisywane bez decyzji użytkownika.
    /// </remarks>
    /// <seealso cref="FileData"/>
    /// <seealso cref="RenamePreview"/>
    public static class Organizer
    {
        /// <summary>
        /// Przenosi albo kopiuje pliki do folderu docelowego zgodnie z wybranymi filtrami i opcjami.
        /// </summary>
        /// <remarks>
        /// Metoda łączy kilka decyzji użytkownika: wybrane typy plików, tryb kopiowania lub przenoszenia,
        /// tworzenie podfolderów oraz podgląd nowych nazw. Gdy plik docelowy już istnieje, konflikt jest
        /// rozwiązywany przez nadpisanie albo dopisanie licznika do nazwy.
        /// </remarks>
        /// <param name="files">Pliki dostępne do organizowania.</param>
        /// <param name="sourceFolder">Folder źródłowy, który musi istnieć przed rozpoczęciem operacji.</param>
        /// <param name="destinationFolder">Folder docelowy; zostanie utworzony, jeśli nie istnieje.</param>
        /// <param name="fileTypes">Lista rozszerzen dopuszczonych do organizowania; pusta lista oznacza brak filtrowania.</param>
        /// <param name="operation">Tryb operacji: "move" oznacza przeniesienie, pozostałe wartości powodują kopiowanie.</param>
        /// <param name="createSubfolders">Określa, czy pliki mają zostać rozdzielone do podfolderow według typu.</param>
        /// <param name="overwriteExisting">Określa, czy istniejące pliki w folderze docelowym mogą zostać nadpisane.</param>
        /// <param name="db">Baza danych używana do zapisu historii operacji; moze byc null.</param>
        /// <param name="previews">Podgląd nowych nazw przygotowany przed wykonaniem operacji.</param>
        /// <returns>Ścieżka folderu docelowego, do którego organizowano pliki.</returns>
        /// <exception cref="DirectoryNotFoundException">Wyrzucany, gdy folder źródłowy nie istnieje.</exception>
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
        /// Wyznacza bezpieczną ścieżkę docelową, gdy w folderze istnieje już plik o tej samej nazwie.
        /// </summary>
        /// <remarks>
        /// Jeśli nadpisywanie jest wyłączone, metoda dodaje licznik w nawiasie do nazwy pliku
        /// i szuka pierwszej wolnej ścieżki.
        /// </remarks>
        /// <param name="fileName">Nazwa pliku bez rozszerzenia.</param>
        /// <param name="fileExt">Rozszerzenie pliku razem z kropką.</param>
        /// <param name="targetPath">Pierwotnie planowana ścieżka docelowa.</param>
        /// <param name="overwriteExisting">Informacja, czy istniejący plik może zostać nadpisany.</param>
        /// <param name="targetFolder">Folder, w którym szukana jest wolna nazwa.</param>
        /// <returns>Ścieżka bez konfliktu albo pierwotna ścieżka, gdy nadpisywanie jest dozwolone.</returns>
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
        /// Dopasowuje rozszerzenie pliku do kategorii używanej przy tworzeniu podfolderow.
        /// </summary>
        /// <remarks>
        /// Kategorie są pobierane z katalogu typów plików. Nieznane rozszerzenia trafiają do folderu "Others",
        /// aby organizowanie nadal dzialało dla formatow spoza listy.
        /// </remarks>
        /// <param name="ext">Rozszerzenie pliku, na przykład ".pdf".</param>
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
