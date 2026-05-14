

namespace FileScannerApp.Models
{
    /// <summary>
    /// Określa rodzaj operacji wykonanej na pliku i zapisanej w historii aplikacji.
    /// </summary>
    /// <remarks>
    /// Typ operacji wpływa na sposob prezentacji wpisu w historii oraz na to, czy dana operacja
    /// może zostać cofnięta. Przeniesienie lub zmiana nazwy zwykle mogą być odwracane,
    /// natomiast trwałe usunięcie blokuje taką możliwość.
    /// </remarks>
    /// <seealso cref="OperationLog"/>
    public enum OperationType
    {
        /// <summary>
        /// Zmiana nazwy pliku bez zmiany jego zawartości.
        /// </summary>
        Rename,

        /// <summary>
        /// Przeniesienie pliku do wewnętrznego kosza aplikacji.
        /// </summary>
        Delete,

        /// <summary>
        /// Przeniesienie pliku do innej lokalizacji.
        /// </summary>
        Move,

        /// <summary>
        /// Utworzenie kopii pliku w lokalizacji docelowej.
        /// </summary>
        Copy,

        /// <summary>
        /// Trwałe usunięcie pliku z kosza aplikacji bez możliwości cofnięcia.
        /// </summary>
        DeletedPermanently
    }
}
