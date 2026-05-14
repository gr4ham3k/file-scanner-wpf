using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp.Models
{
    /// <summary>
    /// Przechowuje bieżący stan długotrwałego skanowania plików.
    /// </summary>
    /// <remarks>
    /// Obiekt jest przekazywany do interfejsu użytkownika w trakcie skanowania, aby można było pokazać
    /// aktualnie analizowany plik, liczbę przetworzonych elementów oraz liczbę wykrytych zagrożeń.
    /// </remarks>
    /// <seealso cref="FileScannerApp.Services.ScanService"/>
    public class ScanProgress
    {
        /// <summary>
        /// Numer aktualnie przetworzonego pliku w ramach całego skanu.
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// Łączna liczba plików przewidzianych do sprawdzenia.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Nazwa pliku, który został ostatnio przekazany do raportowania postępu.
        /// </summary>
        public string CurrentFile { get; set; }

        /// <summary>
        /// Liczba zagrożeń wykrytych od początku aktualnego skanowania.
        /// </summary>
        public int ThreatsFound { get; set; }
    }
}
