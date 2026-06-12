# Frontier Command - Command-Handler-Architektur

## Überblick

Der Command Handler ist für den Empfang, die Validierung und die Speicherung von Spielbefehlen verantwortlich.

Befehle repräsentieren die Absicht eines Spielers oder der KI.

Der Command Handler ist ein zentrales System, da er Client, KI, Mehrspieler und Simulation Core miteinander verbindet.

---

# Zweck

Der Command Handler stellt sicher, dass alle Aktionen über denselben kontrollierten Prozess in das Spiel gelangen, wodurch später unnötiger Netzwerkverkehr vermieden wird.

Befehle können aus folgenden Quellen stammen:

* Eingaben menschlicher Spieler
* Entscheidungen der KI
* Netzwerknachrichten im Mehrspieler
* Debug-Werkzeuge

Alle diese Quellen sollen dasselbe Befehlsformat verwenden.

---

# Befehlsphilosophie

Befehle beschreiben eine Absicht.

Sie sollen visuelle Objekte nicht direkt verändern.

Beispiele:

* Ausgewählte Einheiten zu einer Position bewegen
* Ziel angreifen
* Gebäude errichten
* Einheit ausbilden
* Forschung beginnen
* Außenposten einnehmen
* Sammelpunkt setzen

Der Simulation Core entscheidet, ob ein Befehl gültig ist und wie er ausgeführt wird.

---

# Befehlsablauf

Ein typischer Befehl durchläuft folgenden Prozess:

1. Der Befehl wird erzeugt.
2. Der Befehl wird an den Command Handler gesendet.
3. Der Command Handler validiert den Befehl.
4. Der Befehl wird angenommen oder abgelehnt.
5. Ein angenommener Befehl wird der richtigen Warteschlange hinzugefügt.
6. Der Simulation Core führt die Warteschlange aus.
7. Der Client stellt das Ergebnis dar.

---

# Befehlswarteschlangen

Frontier Command unterstützt das Einreihen von Befehlen in Warteschlangen.

Dadurch können Spieler Einheiten, Gebäuden oder der Forschung mehrere zukünftige Aktionen zuweisen.

---

# Befehlswarteschlange für Einheiten

Einheiten können eine Abfolge von Befehlen speichern.

Beispiel:

1. Zu Position A bewegen.
2. Zu Position B bewegen.
3. Feindliches Gebäude angreifen.
4. Außenposten einnehmen.

Die Einheit führt den ersten Befehl in der Warteschlange aus.

Nach dessen Abschluss beginnt automatisch der nächste Befehl.

---

# Befehlswarteschlange für Gebäude

Auch Gebäude können Warteschlangen speichern.

Beispiele:

* Einheit A ausbilden.
* Einheit B ausbilden.
* Einheit C ausbilden.
* Verbesserung beginnen.

Produktionsgebäude verwenden Warteschlangen, um die Einheitenproduktion und Verbesserungen zu verwalten.

---

# Warteschlangenmodi

## Warteschlange ersetzen

Standardverhalten.

Der neue Befehl ersetzt die aktuelle Warteschlange.

Beispiel:

* Der Spieler klickt mit der rechten Maustaste auf eine Position.
* Die ausgewählten Einheiten brechen ihre bisherigen Befehle ab.
* Die ausgewählten Einheiten bewegen sich zur neuen Position.

---

## Zur Warteschlange hinzufügen

Wird verwendet, wenn der Spieler die Modifikatortaste für die Warteschlange gedrückt hält.

Der neue Befehl wird am Ende der bestehenden Warteschlange hinzugefügt.

Beispiel:

* Der Spieler hält die Umschalttaste gedrückt.
* Der Spieler erteilt mehrere Bewegungsbefehle.
* Die Einheiten folgen der Route der Reihe nach.

---

# Befehlsvalidierung

Jeder Befehl soll vor seiner Ausführung validiert werden.

Mögliche Validierungsprüfungen:

* Gehört die Einheit dem Spieler?
* Ist das Ziel gültig?
* Ist der Befehl für diese Einheit zulässig?
* Sind die erforderlichen Ressourcen verfügbar?
* Ist die Platzierung des Gebäudes gültig?
* Ist die Forschung freigeschaltet?
* Lebt die Einheit?
* Kann der Befehl weiterhin ausgeführt werden?

Ungültige Befehle sollen sauber abgelehnt werden.

---

# Befehlsabschluss

Ein Befehl ist abgeschlossen, wenn sein Ziel erfüllt wurde.

Beispiele:

* Ein Bewegungsbefehl ist abgeschlossen, wenn die Einheit ihr Ziel erreicht.
* Ein Angriffsbefehl ist abgeschlossen, wenn das Ziel zerstört oder unerreichbar ist.
* Ein Baubefehl ist abgeschlossen, wenn der Bau beendet ist.
* Ein Forschungsbefehl ist abgeschlossen, wenn die Technologie freigeschaltet ist.
* Ein Einnahmebefehl ist abgeschlossen, wenn der Außenposten den Besitzer wechselt.

Nach dem Abschluss beginnt der nächste Befehl in der Warteschlange.

---

# Befehlsabbruch

Befehle können abgebrochen werden durch:

* Spielereingaben
* Ungültige Ziele
* Tod einer Einheit
* Zerstörung eines Gebäudes
* Verlust von Ressourcen
* Besitzerwechsel

Abgebrochene Befehle sollen die Warteschlange nicht beschädigen.

Abhängig vom Befehlstyp soll das System entweder mit dem nächsten Befehl fortfahren oder die Warteschlange leeren.

---

# Überlegungen zum Mehrspieler

Der Command Handler soll von Beginn an unter Berücksichtigung des Mehrspielers entworfen werden.

Im Mehrspieler sollen Clients Befehle statt vollständiger Einheitenzustände senden.

Der Server validiert Befehle und verteilt angenommene Befehle.

Dies unterstützt:

* Geringere Bandbreitennutzung
* Besseren Schutz vor Betrug
* Konsistente Spiellogik
