using System;

namespace FileScannerApp.Models
{
    /// <summary>
    /// Reprezentuje pojedynczy wpis historii operacji wykonanej na pliku.
    /// </summary>
    /// <remarks>
    /// Model przechowuje informacj potrzebne do wyświetlenia historii działań użytkownika
    /// oraz do cofnięcia operacji, jeśli jest to możliwe. Szczególnie ważne są ścieżki przed i po operacji,
    /// ponieważ pozwalają odtworzyć poprzednie położenie pliku.
    /// </remarks>
    /// <seealso cref="OperationType"/>
    public class OperationLog
    {
        /// <summary>
        /// Identyfikator wpisu historii w bazie danych.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Rodzaj operacji, który decyduje o sposobie prezentacji i możliwości cofnięcia.
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// Nazwa pliku zapisana w momencie wykonania operacji.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Ścieżka pliku przed wykonaniem operacji.
        /// </summary>
        public string OldPath { get; set; }

        /// <summary>
        /// Ścieżka pliku po wykonaniu operacji.
        /// </summary>
        public string NewPath { get; set; }

        /// <summary>
        /// Data i czas wykonania operacji.
        /// </summary>
        public DateTime OperationDate { get; set; }

        /// <summary>
        /// Informacja, czy aplikacja może bezpiecznie zaproponowac cofnięcie tej operacji.
        /// </summary>
        public bool CanUndo { get; set; }
    }
}
