# Plany rozbudowy

> Sekcja do uzupelnienia po zakonczeniu pierwszej wersji projektu i ocenie brakujacych funkcji.

## Czego zabraklo w pierwszej wersji?

TODO: Wypisz funkcje, ktore byly planowane, ale nie zostaly wykonane w aktualnej wersji.

Przykladowe punkty do rozwazenia:

- bardziej rozbudowane filtrowanie plikow,
- konfiguracja kategorii plikow z poziomu interfejsu,
- eksport wynikow skanowania do pliku CSV/PDF,
- pelniejsze obslugiwanie bledow API VirusTotal,
- wygodniejszy instalator dla uzytkownika koncowego.

## Funkcjonalnosci dla wersji 2.0

TODO: Opisz potencjalne funkcje kolejnej wersji aplikacji.

Propozycje:

- integracja z dodatkowymi silnikami analizy plikow,
- harmonogram automatycznego skanowania folderow,
- system powiadomien po zakonczeniu skanowania,
- zapis ustawien uzytkownika,
- tryb ciemny,
- podglad wiekszej liczby formatow plikow,
- raporty z historii operacji i skanow.

## Potencjalne optymalizacje

TODO: Opisz miejsca, ktore mozna usprawnic technicznie.

Propozycje:

- cache'owanie wynikow skanowania dla plikow o tym samym hashu SHA-256,
- ograniczenie liczby zapytan do API w zaleznosci od limitow VirusTotal,
- lepsze indeksy w bazie SQLite dla historii i wynikow skanowania,
- przeniesienie czesci operacji do zadan w tle,
- poprawa obslugi bardzo duzych folderow,
- rozdzielenie logiki interfejsu od logiki biznesowej.

## Ryzyka i ograniczenia

TODO: Dodaj ograniczenia projektu, ktore warto opisac w dokumentacji.

Przyklady:

- aplikacja wymaga systemu Windows,
- skanowanie wymaga polaczenia z internetem,
- API VirusTotal moze miec limity zapytan,
- aplikacja operuje na rzeczywistych plikach uzytkownika, dlatego operacje przenoszenia i usuwania wymagaja ostroznosci.
