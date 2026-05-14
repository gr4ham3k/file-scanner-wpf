# Uruchomienie projektu dla developera

Ta sekcja opisuje uruchomienie projektu w trybie deweloperskim na czystym komputerze.

## Uzyte technologie

| Technologia | Wersja w projekcie | Zastosowanie | Oficjalna strona |
| --- | --- | --- | --- |
| .NET SDK | 10.0.203 | Budowanie i uruchamianie aplikacji | [.NET](https://dotnet.microsoft.com/) |
| Target Framework | net10.0-windows | Platforma docelowa aplikacji WPF | [.NET target frameworks](https://learn.microsoft.com/dotnet/standard/frameworks) |
| WPF | .NET 10 / Windows | Interfejs aplikacji desktopowej | [WPF](https://learn.microsoft.com/dotnet/desktop/wpf/) |
| Windows Forms | .NET 10 / Windows | Pomocnicze elementy wyboru folderów | [Windows Forms](https://learn.microsoft.com/dotnet/desktop/winforms/) |
| SQLite | System.Data.SQLite compatible package 1.0.119 | Lokalna baza danych aplikacji | [SQLite](https://www.sqlite.org/) |
| Microsoft WebView2 | 1.0.3912.50 | Podgląd PDF/DOCX przez kontrolkę webową | [WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) |
| DocumentFormat.OpenXml | 3.5.1 | Obsługa dokumentów Office | [Open XML SDK](https://learn.microsoft.com/office/open-xml/open-xml-sdk) |
| Mammoth | 1.11.0 | Konwersja DOCX do HTML | [Mammoth .NET](https://github.com/mwilliamson/dotnet-mammoth) |
| OpenXmlPowerTools | 4.5.3.2 | Pomocnicza obsługa dokumentów Open XML | [OpenXmlPowerTools](https://github.com/OfficeDev/Open-Xml-PowerTools) |
| DocFX | 2.78.5 | Generowanie dokumentacji technicznej | [DocFX](https://dotnet.github.io/docfx/) |

## Wymagania programowe

| Wymaganie | Opis |
| --- | --- |
| System operacyjny | Windows 10 lub Windows 11 |
| .NET SDK | .NET SDK 10.0.203 albo kompatybilny SDK .NET 10 |
| Runtime desktopowy | .NET Desktop Runtime zgodny z .NET 10, jeśli aplikacja jest uruchamiana bez SDK |
| WebView2 Runtime | Microsoft Edge WebView2 Runtime wymagany do podglądu PDF/DOCX |
| Git | Narzędzie do pobrania repozytorium |
| Baza danych | Lokalny plik SQLite `Data/database.db`, kopiowany do katalogu wyjściowego |
| DocFX | Opcjonalnie, tylko do generowania dokumentacji |

Projekt jest aplikacją WPF, dlatego nie jest przeznaczony do uruchamiania na macOS ani Linux bez dodatkowych warstw zgodności.

## Proces instalacji

1. Pobierz projekt z repozytorium:

```powershell
git clone <adres-repozytorium>
cd <folder-repozytorium>\FileScannerAppWpf
```

2. Przywróć zależności NuGet:

```powershell
dotnet restore
```

3. Zbuduj projekt:

```powershell
dotnet build
```

4. Uruchom aplikację:

```powershell
dotnet run
```

Aplikacja jest programem desktopowym, więc po uruchomieniu otworzy się okno systemowe. Nie będzie dostępna pod adresem `localhost`, ponieważ nie jest aplikacja webowa.

## Generowanie dokumentacji DocFX

1. Zainstaluj lub zaktualizuj DocFX:

```powershell
dotnet tool update -g docfx
```

2. Wygeneruj metadane API:

```powershell
docfx metadata docfx.json
```

3. Zbuduj stronę dokumentacji:

```powershell
docfx build docfx.json
```

4. Uruchom lokalny serwer dokumentacji:

```powershell
docfx serve _site --port 8080
```

Dokumentacja będzie dostępna pod adresem:

```text
http://localhost:8080
```

Sekcja API będzie dostępna pod adresem:

```text
http://localhost:8080/api/
```
