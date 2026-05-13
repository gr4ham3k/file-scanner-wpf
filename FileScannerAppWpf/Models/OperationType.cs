

namespace FileScannerApp.Models
{
    /// <summary>
    /// Okresla rodzaj operacji wykonanej na pliku i zapisanej w historii aplikacji.
    /// </summary>
    /// <remarks>
    /// Typ operacji wplywa na sposob prezentacji wpisu w historii oraz na to, czy dana operacja
    /// moze zostac cofnieta. Przeniesienie lub zmiana nazwy zwykle moga byc odwracane,
    /// natomiast trwale usuniecie blokuje taka mozliwosc.
    /// </remarks>
    /// <seealso cref="OperationLog"/>
    public enum OperationType
    {
        /// <summary>
        /// Zmiana nazwy pliku bez zmiany jego zawartosci.
        /// </summary>
        Rename,

        /// <summary>
        /// Przeniesienie pliku do wewnetrznego kosza aplikacji.
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
        /// Trwale usuniecie pliku z kosza aplikacji bez mozliwosci cofniecia.
        /// </summary>
        DeletedPermanently
    }
}
