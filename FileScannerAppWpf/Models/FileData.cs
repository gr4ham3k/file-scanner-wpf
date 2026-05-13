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
    /// Model oddziela dane pobrane z systemu plikow od logiki interfejsu i uslug.
    /// Dzieki temu skanowanie, organizowanie oraz wyswietlanie listy plikow korzystaja
    /// z jednej wspolnej struktury danych.
    /// </remarks>
    public class FileData
    {
        /// <summary>
        /// Nazwa pliku pokazywana uzytkownikowi w listach i wynikach operacji.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Rozszerzenie uzywane do filtrowania, organizowania i wyboru sposobu podgladu.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Pelna sciezka potrzebna do wykonania operacji na rzeczywistym pliku.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Rozmiar pliku w bajtach, przydatny przy prezentacji szczegolow pliku.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Data utworzenia wykorzystywana miedzy innymi przy generowaniu nowych nazw.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Data ostatniej modyfikacji wykorzystywana w widoku plikow i wzorcach nazw.
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}
