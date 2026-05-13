# Podrecznik uzytkownika

> Sekcja do uzupelnienia recznie po przygotowaniu zrzutow ekranu z dzialajacej aplikacji.

W tej czesci nalezy opisac aplikacje z perspektywy uzytkownika koncowego, bez wchodzenia w szczegoly kodu.

## Proponowane zrzuty ekranu

TODO: Dodaj zrzuty ekranu do folderu `images/` i osadz je w tej sekcji.

Kazdy zrzut powinien miec krotki opis wyjasniajacy, co widac na ekranie.

Przykladowy zapis:

```markdown
![Glowne okno aplikacji](../images/main-window.png)

Rysunek 1. Glowne okno aplikacji po wybraniu folderu z plikami.
```

## Glowne scenariusze uzytkownika

TODO: Opisz najwazniejsze przeplywy uzytkownika.

Proponowane scenariusze:

- wybor folderu z plikami,
- podglad wybranego pliku,
- skanowanie plikow przez VirusTotal,
- wyswietlenie wynikow skanowania,
- seryjna zmiana nazw plikow,
- organizowanie plikow wedlug typow,
- przenoszenie lub usuwanie plikow,
- przywracanie plikow z historii operacji.

## Zasady dzialania najwazniejszych funkcji

TODO: Wyjasnij, jak dzialaja glowne funkcje aplikacji.

Przykladowe pytania pomocnicze:

- Co dzieje sie po wybraniu folderu?
- Jak aplikacja decyduje, do jakiej kategorii nalezy plik?
- Jak dziala wzorzec zmiany nazw?
- Co oznaczaja statusy skanowania?
- Kiedy operacje mozna cofnac?

## Role w systemie

Aplikacja nie posiada systemu logowania ani podzialu na role. Kazdy uzytkownik korzysta z tego samego zestawu funkcji.

TODO: Jesli w przyszlosci zostanie dodany system kont, opisz role uzytkownikow w tym miejscu.

## Przypadki brzegowe

TODO: Opisz przypadki brzegowe obslugiwane przez aplikacje.

Propozycje do opisania:

- brak internetu podczas skanowania,
- brak klucza API VirusTotal,
- wybranie pustego folderu,
- proba przeniesienia pliku, ktory juz nie istnieje,
- konflikt nazw w folderze docelowym,
- nieobslugiwany format podgladu,
- plik usuniety trwale, ktorego nie mozna juz przywrocic.

## Dane przechowywane przez system

TODO: Opisz dane przechowywane lokalnie przez aplikacje.

Do uwzglednienia:

- historia skanow,
- wyniki skanowania plikow,
- historia operacji na plikach,
- sciezki plikow przed i po operacji,
- odpowiedzi API VirusTotal zapisane przy wynikach skanowania.

## Responsywnosc i mniejsze ekrany

TODO: Aplikacja jest desktopowa WPF, dlatego zamiast responsywnosci webowej opisz zachowanie okien przy zmianie rozmiaru.

Dodaj zrzuty ekranu pokazujace:

- standardowy rozmiar okna,
- zmniejszone okno,
- widok z dluzsza lista plikow,
- okna pomocnicze, takie jak historia albo opcje skanowania.

## Najwazniejszy mechanizm aplikacji

TODO: Opisz najwazniejszy mechanizm projektu.

Proponowany temat:

> Proces skanowania plikow: aplikacja oblicza hash SHA-256, wysyla zapytanie do VirusTotal, zapisuje wynik w bazie danych i pokazuje postep w interfejsie.
