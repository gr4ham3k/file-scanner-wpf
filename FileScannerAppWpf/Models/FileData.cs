using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp.Models
{
    /// <summary>
    /// Reprezentuje podstawowe metadane pliku wykorzystywane przez aplikacje.
    /// </summary>
    /// <remarks>
    /// Model oddziela dane pobrane z systemu plików od logiki interfejsu i usług.
    /// Dzięki temu skanowanie, organizowanie oraz wyświetlanie listy plików korzystają
    /// z jednej wspólnej struktury danych.
    /// </remarks>
    public class FileData
    {
        /// <summary>
        /// Nazwa pliku pokazywana użytkownikowi w listach i wynikach operacji.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Rozszerzenie używane do filtrowania, organizowania i wyboru sposobu podglądu.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Pełna ścieżka potrzebna do wykonania operacji na rzeczywistym pliku.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Rozmiar pliku w bajtach, przydatny przy prezentacji szczegółow pliku.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Data utworzenia wykorzystywana między innymi przy generowaniu nowych nazw.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Data ostatniej modyfikacji wykorzystywana w widoku plików i wzorcach nazw.
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}
