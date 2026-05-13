using System;

namespace FileScannerApp.Models
{
    /// <summary>
    /// Reprezentuje pojedynczy wpis historii operacji wykonanej na pliku.
    /// </summary>
    /// <remarks>
    /// Model przechowuje informacje potrzebne do wyswietlenia historii dzialan uzytkownika
    /// oraz do cofniecia operacji, jesli jest to mozliwe. Szczegolnie wazne sa sciezki przed i po operacji,
    /// poniewaz pozwalaja odtworzyc poprzednie polozenie pliku.
    /// </remarks>
    /// <seealso cref="OperationType"/>
    public class OperationLog
    {
        /// <summary>
        /// Identyfikator wpisu historii w bazie danych.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Rodzaj operacji, ktory decyduje o sposobie prezentacji i mozliwosci cofniecia.
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// Nazwa pliku zapisana w momencie wykonania operacji.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Sciezka pliku przed wykonaniem operacji.
        /// </summary>
        public string OldPath { get; set; }

        /// <summary>
        /// Sciezka pliku po wykonaniu operacji.
        /// </summary>
        public string NewPath { get; set; }

        /// <summary>
        /// Data i czas wykonania operacji.
        /// </summary>
        public DateTime OperationDate { get; set; }

        /// <summary>
        /// Informacja, czy aplikacja moze bezpiecznie zaproponowac cofniecie tej operacji.
        /// </summary>
        public bool CanUndo { get; set; }
    }
}
