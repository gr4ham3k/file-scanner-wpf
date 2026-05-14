# Plany rozbudowy

Ta sekcja opisuje możliwe kierunki rozwoju aplikacji po przygotowaniu pierwszej wersji projektu. Lista nie oznacza błędów aktualnej wersji, tylko pokazuje funkcje, które mogłyby zwiekszyć wygodę, bezpieczeństwo i zakres zastosowań programu.

## Czego zabraklo w pierwszej wersji

W pierwszej wersji aplikacja skupia sie na podstawowym przepływie pracy: wybór folderu, lista plików, podgląd, skanowanie, organizowanie, zmiana nazw i historia operacji. Część funkcji mogłaby zostać rozwinięta w kolejnych etapach.

Najważniejsze braki:

- brak ekranu ustawien aplikacji,
- brak możliwości edycji kategorii plików z poziomu interfejsu,
- brak eksportu historii skanowania do pliku,
- brak kreatora instalacji dla uzytkownika końcowego,
- brak rozbudowanych filtrów, na przykład po dacie, rozmiarze lub nazwie,
- brak mechanizmu zapamiętywania ostatnio używanych folderów,
- brak pełnego panelu diagnostycznego dla problemow z API VirusTotal.

## Funkcjonalnosci dla wersji 2.0

Wersja 2.0 mogłaby rozszerzyć aplikację o funkcje związane z automatyzacją, raportowaniem i personalizacją.

Proponowane funkcje:

| Funkcja | Opis | Korzyść dla użytkownika |
| --- | --- | --- |
| Ustawienia aplikacji | Osobne okno do konfiguracji klucza API, folderów i preferencji | Łatwiejsza konfiguracja bez zmiennych środowiskowych |
| Eksport raportów | Zapis historii skanów do CSV, PDF lub HTML | Możliwość archiwizacji i przekazania wyników dalej |
| Własne kategorie plików | Edycja grup rozszerzeń w interfejsie | Lepsze dopasowanie organizowania do potrzeb użytkownika |
| Harmonogram skanowania | Automatyczne skanowanie wybranego folderu | Mniej recznej pracy przy cyklicznych kontrolach |
| Cache wynikow | Ponowne użycie wyniku dla pliku o tym samym hash'u | Mniej zapytań do API i szybsze skanowanie |
| Tryb ciemny | Alternatywny motyw interfejsu | Większy komfort pracy |
| Rozszerzony podgląd | Obsługa dodatkowych formatów plików | Więcej informacji bez otwierania zewnętrznych programów |
