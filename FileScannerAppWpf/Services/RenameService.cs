using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileScannerApp
{
    /// <summary>
    /// Generuje nowe nazwy plików na podstawie wzorca podanego przez użytkownika.
    /// </summary>
    /// <remarks>
    /// Usługa obsługuje dynamiczne znaczniki, takie jak nazwa pliku, data utworzenia, data modyfikacji,
    /// numer porządkowy, nazwa folderu oraz rozszerzenie. Dzięki temu umożliwia seryjną zmianę nazw
    /// bez ręcznego edytowania każdego pliku osobno.
    /// </remarks>
    /// <seealso cref="RenamePreview"/>
    /// <seealso cref="FileScannerService"/>
    public static class RenameService
    {
        /// <summary>
        /// Buduje nową nazwę pliku na podstawie wzorca i danych oryginalnego pliku.
        /// </summary>
        /// <remarks>
        /// Wzorzec moze zawierać znaczniki: {name}, {created}, {modified}, {counter},
        /// {counter:000}, {folder} oraz {ext}. Oryginalne rozszerzenie jest dopisywane na końcu,
        /// aby zmiana nazwy nie zmieniała typu pliku.
        /// </remarks>
        /// <param name="pattern">Wzorzec nowej nazwy; pusty wzorzec pozostawia nazwę bez zmian.</param>
        /// <param name="originalPath">Pełna ścieżka oryginalnego pliku.</param>
        /// <param name="counter">Numer porządkowy używany w znacznikach licznika.</param>
        /// <param name="option">Opcja formatowania tekstu: "upper", "lower", "capitalize" albo brak zmiany.</param>
        /// <returns>Nowa nazwa pliku wraz z oryginalnym rozszerzeniem.</returns>
        public static string GenerateNewName(string pattern, string originalPath, int counter, string option)
        {
            string name = Path.GetFileNameWithoutExtension(originalPath);
            string ext = Path.GetExtension(originalPath);
            string extNoDot = ext.TrimStart('.');

            if (string.IsNullOrWhiteSpace(pattern))
                return name + ext;

            string created = File.GetCreationTime(originalPath).ToString("yyyy-MM-dd");
            string modified = File.GetLastWriteTime(originalPath).ToString("yyyy-MM-dd");

            string folder = new DirectoryInfo(Path.GetDirectoryName(originalPath)).Name;
            string paddedCounter = counter.ToString("000");

            string newName = pattern
                .Replace("{name}", name)
                .Replace("{created}", created)
                .Replace("{modified}", modified)
                .Replace("{counter:000}", paddedCounter)
                .Replace("{counter}", paddedCounter)
                .Replace("{folder}", folder)
                .Replace("{ext}", extNoDot);

            switch (option)
            {
                case "upper":
                    newName = newName.ToUpper();
                    break;
                case "lower":
                    newName = newName.ToLower();
                    break;
                case "capitalize":
                    if (!string.IsNullOrEmpty(newName))
                        newName = char.ToUpper(newName[0]) + newName.Substring(1).ToLower();
                    break;
            }

            return newName + ext;
        }

        /// <summary>
        /// Przygotowuje listę podglądu nazw dla plików znajdujących się w folderze.
        /// </summary>
        /// <remarks>
        /// Metoda nie zmienia nazw plików na dysku. Tworzy jedynie dane pomocnicze, ktore mogą zostać
        /// pokazane użytkownikowi przed wykonaniem faktycznej operacji zmiany nazw lub organizowania.
        /// </remarks>
        /// <param name="path">Folder, z którego mają zostać pobrane pliki do podglądu.</param>
        /// <returns>Lista obiektow zawierających obecną nazwę pliku oraz miejsce na nazwę po zmianie.</returns>
        public static List<RenamePreview> LoadPreview(string path)
        {
            var files = FileScannerService.Scan(path);

            return files.Select(f => new RenamePreview
            {
                FullPath = f.FullName,
                NameBefore = Path.GetFileName(f.FullName),
                NameAfter = ""
            }).ToList();
        }
    }
}
