# Uruchomienie projektu dla uzytkownika

Ta sekcja opisuje sposób uruchomienia aplikacji przez użytkownika końcowego, który nie pracuje bezpośrednio z kodem źródłowym.

## Pobranie aplikacji

Gotowa wersja aplikacji jest dostępna w sekcji `Releases` repozytorium projektu:

- [File Scanner WPF - V1.0.0](https://github.com/gr4ham3k/file-scanner-wpf/releases/tag/V1.0.0)

Aplikacja jest udostepniona jako archiwum `.zip`. Po pobraniu należy rozpakować paczkę i uruchomić plik `.exe` znajdujący się w rozpakowanym folderze.

## Instalacja

Przykładowy proces instalacji dla użytkownika:

1. Wejdź na stronę wydania: [V1.0.0](https://github.com/gr4ham3k/file-scanner-wpf/releases/tag/V1.0.0).
2. Pobierz plik `.zip` z sekcji `Assets`.
3. Rozpakuj archiwum.
4. Uruchom plik `.exe` z rozpakowanego folderu.

## Wymagania systemowe

| Element | Wymaganie |
| --- | --- |
| System operacyjny | Windows 10 lub Windows 11 |
| Architektura | 64-bit |
| Pamięć RAM | Minimum 4 GB |
| Miejsce na dysku | Minimum 200 MB na aplikacje oraz dodatkowe miejsce na pliki użytkownika |
| Internet | Wymagany do skanowania plików przez VirusTotal |
| WebView2 Runtime | Wymagany do podglądu plików PDF i DOCX |

## Konfiguracja po stronie uzytkownika

Aplikacja korzysta z API VirusTotal. Jeżeli wersja udostępniona użytkownikowi nie zawiera własnego sposobu konfiguracji klucza, należy ustawić zmienną srodowiskową:

```text
VIRUSTOTAL_API_KEY
```

Bez klucza API funkcje niezależne od VirusTotal nadal mogą działać, ale skanowanie plikow nie będzie mogło pobrać raportów z usługi zewnętrznej.
