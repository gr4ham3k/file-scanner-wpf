using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp.Models
{
    /// <summary>
    /// Przechowuje biezacy stan dlugotrwalego skanowania plikow.
    /// </summary>
    /// <remarks>
    /// Obiekt jest przekazywany do interfejsu uzytkownika w trakcie skanowania, aby mozna bylo pokazac
    /// aktualnie analizowany plik, liczbe przetworzonych elementow oraz liczbe wykrytych zagrozen.
    /// </remarks>
    /// <seealso cref="FileScannerApp.Services.ScanService"/>
    public class ScanProgress
    {
        /// <summary>
        /// Numer aktualnie przetworzonego pliku w ramach calego skanu.
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// Laczna liczba plikow przewidzianych do sprawdzenia.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Nazwa pliku, ktory zostal ostatnio przekazany do raportowania postepu.
        /// </summary>
        public string CurrentFile { get; set; }

        /// <summary>
        /// Liczba zagrozen wykrytych od poczatku aktualnego skanowania.
        /// </summary>
        public int ThreatsFound { get; set; }
    }
}
