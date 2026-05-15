# Konfiguracja projektu

Ta sekcja opisuje ustawienia wymagane przed pierwszym uruchomieniem aplikacji w środowisku deweloperskim.

## Zmienne środowiskowe

Aplikacja korzysta z klucza API VirusTotal. Klucz powinien byc zapisany jako zmienna środowiskowa:

```text
VIRUSTOTAL_API_KEY
```

Przykład ustawienia zmiennej w PowerShell dla aktualnej sesji terminala:

```powershell
$env:VIRUSTOTAL_API_KEY="twoj-klucz-api"
```

Przykład ustawienia zmiennej na stałe dla konta użytkownika:

```powershell
[Environment]::SetEnvironmentVariable("VIRUSTOTAL_API_KEY", "twoj-klucz-api", "User")
```

Po ustawieniu zmiennej na stałe należy uruchomic terminal lub Visual Studio ponownie, aby nowa wartość była widoczna dla aplikacji.

## Baza danych

Projekt korzysta z lokalnej bazy SQLite zapisanej w pliku:

```text
Data/database.db
```

Plik bazy jest dołączony do projektu i kopiowany do katalogu wyjściowego podczas budowania aplikacji dzięki konfiguracji w pliku `.csproj`:

```xml
<Content Include="Data\database.db">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>
```

Aplikacja nie wymaga osobnego serwera bazy danych, takiego jak PostgreSQL, MySQL lub SQL Server.

## Connection string

Ścieżka do bazy jest budowana w klasie `Database` na podstawie katalogu uruchomieniowego aplikacji:

```text
<folder-aplikacji>/Data/database.db
```

Nie trzeba ustawiać connection stringa w pliku konfiguracyjnym. Jeśli baza ma zostać przeniesiona w inne miejsce, należy zmienić sposób tworzenia ścieżki w klasie `Database`.

## Migracje

Projekt nie korzysta z migracji Entity Framework. Struktura bazy znajduje się w gotowym pliku SQLite.

Nie ma komendy typu:

```powershell
dotnet ef database update
```

## Dane początkowe

Projekt nie ma mechanizmu seedowania danych testowych ani kont użytkowników. Aplikacja nie posiada systemu logowania, więc nie istnieje domyślne konto administratora.

## Najczęstsze problemy konfiguracyjne

| Problem | Możliwa przyczyna | Rozwiązanie |
| --- | --- | --- |
| Skanowanie nie działa | Brak zmiennej `VIRUSTOTAL_API_KEY` | Ustaw klucz API jako zmienna środowiskowa |
| Podgląd PDF/DOCX nie działa | Brak WebView2 Runtime | Zainstaluj Microsoft Edge WebView2 Runtime |
| Baza nie zapisuje danych | Brak pliku `Data/database.db` w katalogu wyjściowym | Sprawdź, czy plik jest dołączony jako `Content` i kopiowany przy buildzie |
| DocFX nie pokazuje API | Nie wygenerowano metadanych | Uruchom `docfx metadata docfx.json`, a potem `docfx build docfx.json` |
