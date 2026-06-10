# Frontier Command - Simulation-Core-Architektur

## Überblick

Der Simulation Core ist das maßgebliche Herzstück von Frontier Command.

Er ist für die Verwaltung und Aktualisierung des tatsächlichen Spielzustands verantwortlich. Alle Spielregeln, die Weltlogik, Kampfauflösung, Wirtschaftsaktualisierungen, der Forschungsfortschritt, das Einheitenverhalten, die Gebietskontrolle und die Siegbedingungen werden vom Simulation Core verarbeitet.

---

# Zweck

Der Simulation Core dient dazu, die gesamte Spiellogik zentral und konsistent zu halten.

Er soll für Folgendes verwendbar sein:

* Einzelspielerpartien
* Mehrspielerserver
* KI-Partien

---

# Verantwortlichkeiten

Der Simulation Core ist verantwortlich für:

* Speicherung des aktuellen Spielzustands
* Aktualisierung von Einheiten und Gebäuden
* Auflösung von Bewegung
* Auflösung von Kämpfen
* Verwaltung von Ressourcen
* Verarbeitung von Bauvorgängen
* Verarbeitung der Produktion
* Verarbeitung der Forschung
* Verwaltung neutraler Verteidiger
* Prüfung der Sieg- und Niederlagebedingungen
* Erzeugung von Spielereignissen für den Client

---

# Simulationstakt

Der Simulation Core führt das Spiel in festen Simulationsschritten fort.

Eine feste Taktrate erleichtert das Testen, Wiedergeben und Synchronisieren des Spiels sowie die spätere Mehrspielerunterstützung.

Beispielhafter Ablauf eines Takts:

1. Angenommene Befehle empfangen.
2. Neue Befehle anwenden.
3. Bewegung aktualisieren.
4. Kämpfe aktualisieren.
5. Bauvorgänge aktualisieren.
6. Produktion aktualisieren.
7. Forschung aktualisieren.
8. Wirtschaft aktualisieren.
9. Territorium und Außenposten aktualisieren.
10. Siegbedingungen prüfen.
11. Spielereignisse ausgeben.

---

# Spielzustand

Der Simulation Core besitzt den maßgeblichen Spielzustand.

Dazu gehören:

* Spieler
* Fraktionen
* Ressourcen
* Einheiten
* Gebäude
* Projektile
* Forschungsstatus
* Produktionswarteschlangen
* Befehlswarteschlangen
* Außenposten
* Neutrale Verteidiger
* Kriegsnebel
* Spielzeit
* Siegstatus

Andere Systeme dürfen diesen Zustand lesen, aber nur der Simulation Core soll ihn direkt verändern.

---

# Kernsysteme

## Einheitensystem

Verwaltet die gesamte einheitenbezogene Logik.

Verantwortlichkeiten:

* Erzeugung von Einheiten
* Zerstörung von Einheiten
* Bewegung
* Wegfindungsanfragen
* Ausführung von Befehlswarteschlangen
* Trefferpunkte
* Statuseffekte
* Besitz von Einheiten

---

## Gebäudesystem

Verwaltet Gebäude.

Verantwortlichkeiten:

* Validierung der Gebäudeplatzierung
* Baufortschritt
* Besitz von Gebäuden
* Zerstörung von Gebäuden
* Gebäudeverbesserungen
* Energieverbrauch von Gebäuden
* Gebäudefunktionalität

---

## Wirtschaftssystem

Verwaltet die Ressourcen der Spieler.

Verantwortlichkeiten:

* Materialgewinnung
* Energieerzeugung
* Ressourcenausgaben
* Ressourcenspeicherung
* Wirtschaftliche Boni
* Wirtschaftliche Effekte von Außenposten

---

## Forschungssystem

Verwaltet den technologischen Fortschritt.

Verantwortlichkeiten:

* Verfügbarkeit von Forschung
* Forschungskosten
* Forschungsfortschritt
* Auswahl von Doktrinen
* Freischaltung von Technologien
* Forschungseffekte

---

## Kampfsystem

Verwaltet die Kampfauflösung.

Verantwortlichkeiten:

* Zielvalidierung
* Abklingzeiten von Waffen
* Schadensberechnung
* Wechselwirkung mit Panzerung
* Erzeugung von Projektilen
* Flächenschaden
* Zerstörung von Einheiten

Kämpfe sollen verständlich, vorhersehbar und konsistent sein.

---

## Bewegungssystem

Verwaltet die Bewegung von Einheiten.

Verantwortlichkeiten:

* Einheiten zu Zielen bewegen
* Formationen verwalten
* Kollisionen vermeiden
* Auf Ergebnisse der Wegfindung reagieren
* Große Armeen unterstützen

Die Bewegung muss effizient sein, da Frontier Command auf mittelgroße bis große Schlachten mit bis zu ungefähr 150 Einheiten pro Spieler im späten Spiel ausgerichtet ist.

---

## Neutrales System

Verwaltet neutrale Verteidiger und unkontrollierte Außenposten.

Verantwortlichkeiten:

* Verhalten neutraler Einheiten
* Verteidigungsgruppen von Außenposten
* Regeln für das erneute Erscheinen neutraler Einheiten, falls zutreffend
* Aggressionsregeln neutraler Einheiten

Neutrale Streitkräfte sollen eine kostenlose Expansion begrenzen und Ziele für das frühe Spiel schaffen.

---

## Kriegsnebelsystem

Verwaltet die Sicht der Spieler.

Verantwortlichkeiten:

* Sichtbare Gebiete
* Zuvor erkundete Gebiete
* Verborgene feindliche Bewegungen
* Entdeckungsregeln
* Bedeutung von Aufklärungseinheiten

Der Kriegsnebel ist entscheidend für Aufklärung, Überraschungsangriffe und strategische Unsicherheit.

---

## Siegsystem

Verwaltet den Abschluss einer Partie.

Verantwortlichkeiten:

* Prüfung der Siegbedingungen
* Prüfung der Niederlagebedingungen
* Eliminierung von Spielern
* Teamsieg
* Ereignisse am Ende einer Partie

Siegbedingungen sollen klar und vorhersehbar sein.

---

# Ereignisse

Der Simulation Core soll Ereignisse ausgeben, wenn wichtige Dinge geschehen.

Beispiele:

* Einheit erzeugt
* Einheit zerstört
* Gebäude fertiggestellt
* Forschung abgeschlossen
* Außenposten eingenommen
* Spieler angegriffen
* Ressource erschöpft
* Sieg errungen

Der Client kann diese Ereignisse für Folgendes verwenden:

* Visuelle Effekte
* Soundeffekte
* Warnmeldungen der Benutzeroberfläche
* Benachrichtigungen
* Wiederholungen

Ereignisse sollen beschreiben, was geschehen ist, ohne von Darstellungsdetails abhängig zu sein.

---

# Überlegungen zum Mehrspieler

Im Mehrspieler soll der Simulation Core auf dem Server ausgeführt werden.

Clients senden Befehle an den Server.

Der Server validiert Befehle, aktualisiert den Simulation Core und sendet Zustandsaktualisierungen oder Ereignisse an die Clients zurück.

Clients sollen keine maßgebliche Kontrolle über Spielergebnisse besitzen.

Dies verhindert viele Formen von Betrug und hält die Spiellogik konsistent.

---

# Überlegungen zum Einzelspieler

Im Einzelspieler wird der Simulation Core lokal in derselben Anwendung wie der Client ausgeführt.

Die Architektur soll gleich bleiben:

* Spielereingaben erzeugen Befehle.
* Die KI erzeugt Befehle.
* Der Command Handler validiert Befehle.
* Der Simulation Core aktualisiert das Spiel.

Dadurch verwenden Einzelspieler und Mehrspieler dieselbe Spiellogik.
