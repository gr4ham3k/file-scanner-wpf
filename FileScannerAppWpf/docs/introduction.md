# Temat projektu

FileScannerAppWpf to aplikacja desktopowa WPF sluzaca do zarzadzania plikami w wybranym folderze. Program pozwala skanowac pliki przez VirusTotal, porzadkowac je wedlug typow, seryjnie zmieniac nazwy oraz sledzic historie wykonanych operacji.

Projekt jest przeznaczony dla osob, ktore pracuja z wieloma lokalnymi plikami i chca ograniczyc reczne, powtarzalne czynnosci wykonywane w eksploratorze systemowym. Glowne zastosowanie aplikacji to szybkie sprawdzenie, uporzadkowanie i opisanie plikow bez koniecznosci korzystania z kilku osobnych narzedzi.

## Jaki problem rozwiazuje aplikacja?

Aplikacja rozwiazuje problem chaotycznych folderow, w ktorych znajduje sie wiele plikow roznych typow. Uzytkownik moze w jednym miejscu:

- wyswietlic metadane plikow,
- sprawdzic pliki pod katem zagrozen,
- podgladac wybrane formaty,
- przenosic lub kopiowac pliki do folderu docelowego,
- automatycznie tworzyc podfoldery wedlug kategorii,
- seryjnie zmieniac nazwy plikow wedlug wzorca,
- przegladac historie skanow i operacji.

## Czym aplikacja sie wyroznia?

Podobne zadania mozna wykonac przy pomocy eksploratora plikow, skryptow albo osobnych programow do zmiany nazw i skanowania. FileScannerAppWpf laczy te funkcje w jednym interfejsie oraz zapisuje historie dzialan w lokalnej bazie SQLite.

Istotnym elementem projektu jest rowniez skanowanie po skrocie SHA-256. Aplikacja nie musi wysylac zawartosci pliku do zewnetrznej uslugi, tylko sprawdza raport VirusTotal na podstawie hasha pliku.
