# Frontier Command - Client-Architektur

## Überblick

Der Client ist dafür verantwortlich, dem Spieler das Spiel darzustellen und Spieleraktionen in Spielbefehle umzuwandeln.

Der Client soll keine maßgeblichen Spielregeln enthalten. Seine Hauptaufgabe besteht in der Darstellung, Eingabeverarbeitung, Kamerasteuerung, Benutzeroberfläche, Audioausgabe und Rückmeldung an den Spieler.

Der eigentliche Spielzustand soll vom Simulation Core verwaltet werden.

---

# Verantwortlichkeiten

Der Client ist verantwortlich für:

* Darstellung des Schlachtfelds
* Darstellung von Einheiten, Gebäuden, Effekten und Benutzeroberfläche
* Verarbeitung von Spielereingaben
* Verwaltung der Kamera
* Anzeige von Auswahl- und Befehlsrückmeldungen
* Wiedergabe von Geräuschen und Musik
* Anzeige von Spielinformationen
* Erzeugung von Befehlen aus Spieleraktionen

Der Client soll nicht verantwortlich sein für:

* Entscheidung von Kampfergebnissen
* Verwaltung von Ressourcenregeln
* Auflösung von Forschungseffekten
* Bestimmung von Sieg oder Niederlage
* Ausführung maßgeblicher Mehrspielerlogik

---

# Zentrale Client-Systeme

## Darstellungssystem

Stellt den aktuellen Spielzustand dar.

Verantwortlichkeiten:

* Gelände darstellen
* Einheiten darstellen
* Gebäude darstellen
* Projektile darstellen
* Effekte darstellen
* Auswahlmarkierungen darstellen
* Kriegsnebel darstellen

Das Darstellungssystem soll den Spielzustand lesen, ihn aber nicht direkt verändern.

---

## Eingabesystem

Wandelt Spieleraktionen in Absichten um.

Beispiele:

* Einheiten auswählen
* Kamera bewegen
* Bewegungsbefehl erteilen
* Angriffsbefehl erteilen
* Gebäude platzieren
* Forschung beginnen
* Einheit ausbilden

Eingaben sollen Spielbefehle erzeugen, statt Spielobjekte direkt zu verändern.

---

## Kamerasystem

Steuert die Navigation über das Schlachtfeld.

Verantwortlichkeiten:

* Kamera bewegen
* Kamerazoom steuern
* Kamera innerhalb der Kartengrenzen halten

Für Frontier Command soll die Kamera einen festen Blickwinkel besitzen.

Eine Drehung der Kamera ist nicht erforderlich.

---

## Auswahlsystem

Verarbeitet die Auswahl von Einheiten und Gebäuden.

Verantwortlichkeiten:

* Einzelauswahl
* Rahmenauswahl
* Gruppenauswahl
* Auswahlfilterung
* Anzeige von Informationen zur ausgewählten Einheit

Die Auswahl soll clientseitig erfolgen, da sie nur für den lokalen Spieler relevant ist.

---

## System für Befehlsrückmeldungen

Stellt eine visuelle Bestätigung von Spielerbefehlen bereit.

Beispiele:

* Markierung für Bewegungsbefehle
* Markierung für Angriffsbefehle
* Vorschau der Gebäudeplatzierung
* Warnung bei ungültiger Platzierung
* Sammelpunktmarkierung

Gute Rückmeldungen sind entscheidend für eine reaktionsschnelle RTS-Steuerung.

---

## Benutzeroberflächensystem

Zeigt Spielerinformationen und Steuerelemente an.

Verantwortlichkeiten:

* Ressourcen
* Energie
* Einheitenproduktion
* Gebäudeproduktion
* Forschungsoptionen
* Informationsfeld der ausgewählten Einheit
* Minikarte
* Warnmeldungen
* Sieg- und Niederlagebildschirm

Die Benutzeroberfläche soll Informationen klar darstellen, ohne den Spieler zu überfordern.

---

## Audiosystem

Stellt akustische Rückmeldungen bereit.

Verantwortlichkeiten:

* Bestätigungen von Einheiten
* Kampfgeräusche
* Baugeräusche
* Warnsignale
* Musik
* Geräusche der Benutzeroberfläche

Audio soll dem Spieler helfen, das Geschehen zu verstehen, ohne ständig jeden Teil der Karte beobachten zu müssen.

---

# Trennung von Client und Simulation

Der Client soll über Befehle mit dem Simulation Core kommunizieren.

Beispielablauf:

1. Der Spieler wählt Einheiten aus.
2. Der Spieler klickt mit der rechten Maustaste auf eine Position.
3. Der Client erzeugt einen Bewegungsbefehl.
4. Der Simulation Core validiert den Befehl.
5. Der Simulation Core aktualisiert die Einheitenbefehle.
6. Der Client stellt das Ergebnis dar.

Diese Trennung ist wichtig, da dasselbe Befehlssystem später Folgendes unterstützen kann:

* KI-Spieler
* Mehrspieler
* Tutorials
* Automatisierte Tests

---

# Befehlsbeispiele

Häufige Spielerbefehle sind:

* Einheiten bewegen
* Ziel angreifen
* Einheiten anhalten
* Gebiet patrouillieren
* Gebäude errichten
* Gebäude reparieren
* Einheit ausbilden
* Forschung beginnen
* Außenposten einnehmen
* Sammelpunkt setzen

Befehle sollen die Absicht des Spielers und keine visuellen Effekte beschreiben.

---

# Clientseitiger Zustand

Einige Informationen existieren nur auf dem Client.

Beispiele:

* Aktuelle Kameraposition
* Aktuelle Auswahl
* Zustand der Benutzeroberflächenfelder
* Mausposition
* Lokale Kontrollgruppen
* Lokale Tastenkürzel
* Visuelle Vorschauen

Dieser Zustand muss im Mehrspieler nicht synchronisiert werden.

---
