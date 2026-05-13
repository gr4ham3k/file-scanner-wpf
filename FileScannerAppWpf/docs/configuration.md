# Konfiguracja projektu

Ta sekcja opisuje ustawienia wymagane przed pierwszym uruchomieniem aplikacji w srodowisku deweloperskim.

## Zmienne srodowiskowe

Aplikacja korzysta z klucza API VirusTotal. Klucz powinien byc zapisany jako zmienna srodowiskowa:

```text
VIRUSTOTAL_API_KEY
```

Przyklad ustawienia zmiennej w PowerShell dla aktualnej sesji terminala:

```powershell
$env:VIRUSTOTAL_API_KEY="twoj-klucz-api"
```

Przyklad ustawienia zmiennej na stale dla konta uzytkownika:

```powershell
[Environment]::SetEnvironmentVariable("VIRUSTOTAL_API_KEY", "twoj-klucz-api", "User")
```

Po ustawieniu zmiennej na stale nalezy uruchomic terminal lub Visual Studio ponownie, aby nowa wartosc byla widoczna dla aplikacji.

## Baza danych

Projekt korzysta z lokalnej bazy SQLite zapisanej w pliku:

```text
Data/database.db
```

Plik bazy jest dolaczony do projektu i kopiowany do katalogu wyjsciowego podczas budowania aplikacji dzieki konfiguracji w pliku `.csproj`:

```xml
<Content Include="Data\database.db">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>
```

Aplikacja nie wymaga osobnego serwera bazy danych, takiego jak PostgreSQL, MySQL lub SQL Server.

## Connection string

Sciezka do bazy jest budowana w klasie `Database` na podstawie katalogu uruchomieniowego aplikacji:

```text
<folder-aplikacji>/Data/database.db
```

Nie trzeba ustawiac connection stringa w pliku konfiguracyjnym. Jesli baza ma zostac przeniesiona w inne miejsce, nalezy zmienic sposob tworzenia sciezki w klasie `Database`.

## Migracje

Projekt nie korzysta z migracji Entity Framework. Struktura bazy znajduje sie w gotowym pliku SQLite.

Nie ma komendy typu:

```powershell
dotnet ef database update
```

## Dane poczatkowe

Projekt nie ma mechanizmu seedowania danych testowych ani kont uzytkownikow. Aplikacja nie posiada systemu logowania, wiec nie istnieje domyslne konto administratora.

## Najczestsze problemy konfiguracyjne

| Problem | Mozliwa przyczyna | Rozwiazanie |
| --- | --- | --- |
| Skanowanie nie dziala | Brak zmiennej `VIRUSTOTAL_API_KEY` | Ustaw klucz API jako zmienna srodowiskowa |
| Podglad PDF/DOCX nie dziala | Brak WebView2 Runtime | Zainstaluj Microsoft Edge WebView2 Runtime |
| Baza nie zapisuje danych | Brak pliku `Data/database.db` w katalogu wyjsciowym | Sprawdz, czy plik jest dolaczony jako `Content` i kopiowany przy buildzie |
| DocFX nie pokazuje API | Nie wygenerowano metadanych | Uruchom `docfx metadata docfx.json`, a potem `docfx build docfx.json` |
