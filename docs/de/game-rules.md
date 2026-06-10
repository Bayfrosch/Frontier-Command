# Frontier Command - Spielregeln

## Zweck

Dieses Dokument definiert die ersten konkreten Spielregeln für Frontier Command.

Diese Werte sind keine endgültigen Balancewerte. Sie dienen dazu, den ersten spielbaren Prototyp konsistent, testbar und messbar zu machen.

---

# Spieleinstellungen

## Angestrebte Spieldauer

Ein normales Spiel sollte folgende Dauer haben:

* Kurzes Spiel: 20-40 Minuten
* Normales Spiel: 0,5-2 Stunden
* Langes Spiel: mehr als 2 Stunden

Das standardmäßige Designziel beträgt:

**1,5 Stunden**

---

## Spieleranzahl

Die anfängliche Unterstützung sollte sich auf Folgendes konzentrieren:

* 1v1
* 2v2
* Jeder gegen jeden

---

## Kartengrößen

Die Kartengröße sollte mit der Spieleranzahl skalieren.

Empfohlene anfängliche Größen:

* Klein, 1v1: 128 x 128 Felder
* Standard, 1v1: 192 x 192 Felder
* 2v2: 256 x 256 Felder
* Große Teamkarte: 320 x 320 Felder

Ein Feld stellt eine logische Spieleinheit dar, nicht notwendigerweise ein gerendertes Pixel.

---

# Startbedingungen

Jeder Spieler beginnt mit:

* 1 Kommandozentrale
* 1 Konstruktionseinheit
* 1 Aufklärungseinheit
* 15000 Materialien

Die Starteinheiten sollten Spielern Folgendes ermöglichen:

* Mit dem Basisbau beginnen
* Mit dem Aufbau einer Armee beginnen
* Die Karte erkunden

---

# Ressourcen

Frontier Command verwendet zwei Hauptressourcen:

## Materialien

Verwendet für:

* Gebäude
* Einheiten
* Forschung
* Verteidigungsanlagen

## Energie

Energie ist eine feste Kapazitätsressource. Sie wird weder angesammelt noch ausgegeben.

Kraftwerke stellen Energiekapazität bereit und energieabhängige Gebäude benötigen Energiekapazität.

```text
Verfügbare Energie = Gesamte Energieproduktion - Gesamter Energiebedarf
```

Regeln für den Energiestatus:

* Wenn die Gesamtproduktion mindestens dem Gesamtbedarf entspricht, werden alle Gebäude mit Energie versorgt.
* Wenn der Gesamtbedarf die Gesamtproduktion übersteigt, hat der Spieler Energiemangel.
* Einheiten benötigen keine Energie, sofern dies nicht ausdrücklich angegeben ist.
* Die Zerstörung oder Deaktivierung eines Kraftwerks aktualisiert sofort die verfügbare Energie des Spielers.
* Das Abbrechen oder Zerstören eines solchen Gebäudes gibt die reservierte Energie frei.

Anfängliche Regel bei Energiemangel:

* Produktions- und Forschungsgebäude pausieren.
* Verteidigungsanlagen stellen das Feuern ein.
* Aufklärungsgebäude können keine versteckten Einheiten mehr aufdecken.
* Der Bau wird mit 50 % Geschwindigkeit fortgesetzt.
* Kraftwerke und Kommandozentralen bleiben in Betrieb.

---

# Einkommensregeln

## Materialien

Materialien werden von Sammlern aus Ressourcenfeldern abgebaut.

Grundregel:

* 1 Sammler transportiert 1000 Materialien pro Fahrt.
* 1 vollständiger Sammelzyklus dauert ungefähr 30 Sekunden.

Eine Startwirtschaft sollte die grundlegende Produktion ermöglichen, jedoch nicht gleichzeitig eine schnelle Expansion und Forschung.

---

## Energie

Energie wird von Energiegebäuden erzeugt.

Grundregel:

* Ein einfacher Generator stellt 10 Energiekapazität bereit.
* Ein fortschrittlicher Generator stellt 25 Energiekapazität bereit.

Energie sollte im mittleren und späten Spiel zunehmend wichtiger werden.

---

# Ressourcenerschöpfung

Materialfelder sollten endlich sein.

Startressourcenfeld: 200.000 Materialien

Später sollte jede Fraktion eine zuverlässige Möglichkeit freischalten, unbegrenzt Materialien zu produzieren.

Energie erschöpft sich nicht, erfordert jedoch Investitionen in die Infrastruktur.

---

# Startkosten

Anfängliche ungefähre Kosten:

| Objekt                | Materialien | Energiebedarf | Zeit |
| --------------------- | ----------: | ------------: | ---: |
| Konstruktionseinheit  |        1000 |             0 |  20s |
| Sammler               |         600 |             0 |  25s |
| Einfacher Generator   |         800 |             0 |  25s |
| Raffinerie            |        2000 |             0 |  35s |
| Kaserne               |         600 |             0 |  30s |
| Fahrzeugfabrik        |        2000 |             5 |  45s |
| Flugfeld              |        1000 |             5 |  45s |
| Forschungszentrum (A) |        3000 |            30 |  60s |
| Forschungszentrum (C) |        3000 |            30 |  60s |
| Einfache Verteidigung |        1000 |            15 |  25s |
| Materialquelle        |        2500 |            50 |  25s |

---

# Richtwerte für Einheitenkosten

| Einheitenrolle          | Materialien | Produktionszeit |
| ----------------------- | ----------: | --------------: |
| Infanterie              |         300 |             12s |
| Leichte Kampfeinheit    |         600 |             15s |
| Hauptkampfeinheit       |         800 |             28s |
| Panzerabwehreinheit     |         800 |             32s |
| Artillerieeinheit       |        1600 |             45s |
| Flugabwehreinheit       |         800 |             30s |
| Unterstützungseinheit   |         600 |             40s |
| Schwere Angriffseinheit |        2000 |             60s |
| Kapitaleinheit          |        5000 |            120s |

---

# Forschungsregeln

Forschung sollte teuer genug sein, um eine bewusste Verpflichtung zu erfordern.

## Forschungsstufen

### Stufe 1 - Grundlagenforschung

* Kosten: 1000-1500 Materialien, 15-30 Energie
* Zeit: 45-90 Sekunden
* Zweck: grundlegende Verbesserungen und Freischaltungen

### Stufe 3 - Doktrinforschung

* Kosten: 1500-3500 Materialien, 50 Energie
* Zeit: 120-180 Sekunden
* Zweck: mächtige Einheiten, Gebäude und Systeme freischalten

Forschung sollte in direkter Konkurrenz zur Armeeproduktion stehen.

### Flottengrößenforschung

* Kosten: feste Kosten plus laufende Unterhaltskosten von 500->5000 Materialien, 10 Energie plus 5 %->50 % des Materialeinkommens

---

# Produktions- und Technologieregeln

## Voraussetzungen

Jede Einheit, jedes Gebäude und jedes Forschungsprojekt besitzt eine ausdrückliche Liste von Voraussetzungen.

Anfängliche Kette von Voraussetzungen:

1. Die Kommandozentrale schaltet Konstruktionseinheiten, einfache Generatoren, Raffinerien und Kasernen frei.
2. Die Kaserne schaltet Infanterie, Aufklärungs- und Gesandteneinheiten frei.
3. Die Raffinerie schaltet Sammler frei.
4. Ein einfacher Generator und eine Kaserne schalten die Fahrzeugfabrik frei.
5. Ein einfacher Generator und eine Fahrzeugfabrik schalten das Flugfeld und die Kapitalfabrik frei.
6. Ein einfacher Generator und ein grundlegendes Produktionsgebäude schalten Forschungszentren für Armee und Zivilbereich frei.

Der Verlust einer Voraussetzung zerstört oder deaktiviert bereits fertiggestellte Einheiten und Gebäude nicht. Er verhindert den Start neuer abhängiger Produktionen oder Forschungen, bis die Voraussetzung wiederhergestellt wurde.

## Produktionswarteschlangen

* Jedes Produktionsgebäude besitzt eine Warteschlange.
* Eine Warteschlange kann bis zu 99 Objekte enthalten.
* Das erste Objekt in der Warteschlange wird produziert; spätere Objekte warten in ihrer Reihenfolge.
* Verschiedene Produktionsgebäude arbeiten parallel.
* Fertiggestellte Bodeneinheiten erscheinen am Ausgang des Gebäudes und bewegen sich zu dessen Sammelpunkt.

## Bezahlung, Abbruch und Zerstörung

* Die vollständigen Materialkosten werden beim Start der Produktion oder Forschung bezahlt.
* Ein Objekt kann nicht begonnen werden, wenn seine Kosten, Voraussetzungen oder Flottenkapazitätsanforderungen nicht erfüllt sind.
* Der Abbruch einer unfertigen Einheit oder eines Forschungsprojekts erstattet 100 % der Materialkosten.
* Der Abbruch eines wartenden Objekts erstattet 100 %.
* Wird das produzierende Gebäude zerstört, geht das aktive Objekt ohne Erstattung verloren.
* Wartende Objekte werden bei der Zerstörung ihres Produktionsgebäudes abgebrochen und zu 100 % erstattet.
* Eine pausierte Produktion behält ihren aktuellen Fortschritt.

## Flottengröße

* Jede Kampfeinheit verbraucht Flottenkapazität.
* Arbeiter und Sammler verbrauchen anfänglich keine Flottenkapazität.
* Eine Einheit reserviert ihre Kapazität, sobald ihre Produktion beginnt.
* Zerstörte oder abgebrochene Einheiten geben ihre reservierte Kapazität sofort frei.
* Bestehende Einheiten bleiben aktiv, wenn die Flottenkapazitätsgrenze des Spielers unter die aktuelle Nutzung sinkt.
* Es darf keine neue Einheit produziert werden, bis die Nutzung die Grenze nicht mehr überschreitet.

## Sammelpunkte

* Produktionsgebäude können einen Sammelpunkt besitzen.
* Ein Sammelpunkt kann auf eine gültige Position gesetzt werden.
* Neu produzierte Einheiten erhalten einen Bewegungsbefehl zu diesem Ziel.

---

# Bauregeln

## Konstruktionseinheiten

Konstruktionseinheiten errichten alle Gebäude.

Regeln:

* Gebäude können überall auf gültigem Gelände platziert werden.
* Die Konstruktionseinheit muss sich zum Bauort bewegen.
* Der Bau beginnt, sobald die Konstruktionseinheit den Bauort erreicht.

---

# Bewegungsregeln

## Bewegungskategorien

Einheiten verwenden eine der folgenden Bewegungskategorien:

* Boden
* Schwebend
* Luft

Bodeneinheiten bewegen sich auf passierbarem Gelände und können blockiertes Gelände nicht überqueren. Schwebende Einheiten dürfen festgelegte Flachwasser- und Schlechtgeländebereiche überqueren. Lufteinheiten ignorieren Bodenhindernisse, bleiben jedoch innerhalb der Kartengrenzen.

## Bewegungsausführung

* Die Bewegungsgeschwindigkeit wird für jede Einheit festgelegt.
* Einheiten beschleunigen und bremsen, statt ihre Geschwindigkeit sofort zu ändern.
* Einheiten drehen sich mit ihrer festgelegten Drehgeschwindigkeit in ihre Bewegungsrichtung.
* Ein Bewegungsbefehl ist abgeschlossen, sobald die Einheit ihren Ankunftsradius erreicht.
* Einheiten werden in der Nähe ihres Ziels langsamer, um ein Überschießen und Staus zu reduzieren.
* Eine Einheit, die das genaue Ziel nicht erreichen kann, bewegt sich zum nächstgelegenen erreichbaren Punkt.
* Wird nach drei Versuchen kein gültiger Pfad gefunden, schlägt der Befehl fehl und der Client erhält ein Ereignis für ein unerreichbares Ziel.

## Kollision und Abstand

* Verbündete Bodeneinheiten verwenden lokale Ausweichbewegungen, um Überschneidungen zu verhindern.
* Einheiten dürfen verbündete Einheiten sanft wegdrücken, wenn diese untätig sind oder sich in dieselbe Richtung bewegen.
* Einheiten können keine gegnerischen Einheiten, Gebäude oder bewegungsunfähigen Einheiten wegdrücken.
* Große Einheiten erhalten eine höhere Ausweichpriorität als kleine Einheiten.
* Einheiten dürfen nicht dauerhaft von verbündeten Einheiten eingeschlossen werden.
* Lufteinheiten kollidieren nicht mit Bodeneinheiten.

## Gruppenbewegung und Formationen

* Ein Gruppenbewegungsbefehl weist Positionen um das Ziel herum zu.
* Schnellere Einheiten verringern bei Bedarf ihre Geschwindigkeit, damit die Gruppe angemessen zusammenbleibt.
* Der erste Prototyp verwendet eine einfache rechteckige Formation.
* Einheiten wählen Formationspositionen, die ihrer Größe und Bewegungskategorie entsprechen.
* Einheiten dürfen die Formation vorübergehend verlassen, um Hindernissen auszuweichen oder Ziele anzugreifen.
* Formationsbewegungen dürfen den Spieler nicht daran hindern, einzelnen Einheiten Befehle zu erteilen.

## Bewegung im Kampf

* Eine Einheit darf sich während des Feuerns nur bewegen, wenn ihre Waffe dies erlaubt.
* Waffen, die zum Feuern Stillstand erfordern, verhindern während ihrer Feuersequenz die Bewegung.
* Ein Angriff-Bewegungs-Befehl greift gültige Gegner an, die auf dem Weg angetroffen werden.
* Wenn das Ziel eines Angriff-Bewegungs-Befehls verloren geht oder zerstört wird, setzt die Einheit ihren Weg fort.
* Bei einem direkten Angriffsbefehl darf eine Einheit ihr Ziel bis zu ihrer Verfolgungsdistanz verfolgen.
* Wird die Verfolgungsdistanz überschritten, kehrt die Einheit zu ihrer vorherigen Position oder ihrem vorherigen Befehl zurück.

---

# Reparaturregeln

Konstruktionseinheiten können Gebäude und mechanische Einheiten reparieren.

Grundregel:

* Eine Reparatur stellt 40 Trefferpunkte pro Sekunde wieder her.
* Eine vollständige Reparatur kostet 25 % der ursprünglichen Kosten der Einheit oder des Gebäudes.
* Eine Reparatur kann durch Schaden unterbrochen werden.
* Eine Reparatur unter Beschuss ist möglich, aber ineffizient.

---

# Kampfregeln

## Grundformel

Schaden wird wie folgt berechnet:

```text
Endschaden = Grundschaden x Rüstungsmodifikator - Grundverteidigung
```

Der minimale Endschaden beträgt immer 1.

Berechnungsregeln:

* Der Rüstungsmodifikator wird anhand des Modifikators der Waffe gegen die Rüstungsklasse des Ziels ausgewählt.
* Die Grundverteidigung ist eine vom Ziel festgelegte, pauschale Schadensreduktion.
* Die Multiplikation wird vor dem Abzug der Grundverteidigung angewendet.
* Ein Bruchteil des Endschadens wird auf die nächste ganze Zahl gerundet.
* Schadensmodifikatoren durch Veteranenstatus, Fähigkeiten und Statuseffekte werden vor der Rüstungsberechnung auf den Grundschaden angewendet.
* Ein zerstörtes Ziel kann keinen weiteren Schaden erhalten.

Anfängliche Matrix der Rüstungsmodifikatoren:

| Waffenklasse   | Leicht | Mittel | Schwer | Gebäude | Luft | Kapital |
| -------------- | -----: | -----: | ------: | -------: | ---: | ------: |
| Handfeuerwaffe |   1.25 |   0.75 |    0.40 |     0.30 | 0.00 |    0.25 |
| Kanone         |   1.00 |   1.00 |    0.85 |     0.75 | 0.00 |    0.65 |
| Panzerabwehr   |   0.75 |   1.25 |    1.50 |     1.00 | 0.00 |    1.25 |
| Explosiv       |   1.25 |   1.00 |    0.75 |     1.25 | 0.00 |    0.75 |
| Flugabwehr     |   0.25 |   0.25 |    0.25 |     0.10 | 1.50 |    0.50 |

Ein Modifikator von `0.00` bedeutet, dass die Waffe diese Rüstungsklasse nicht als Ziel wählen kann.

---

## Angriffsablauf

Jede Waffe definiert:

* Vorbereitungszeit
* Abklingzeit
* Grundschaden
* Genauigkeit
* Minimale und maximale Reichweite
* Gültige Zieltypen
* Projektil- oder Soforttrefferverhalten

Anfängliche Regeln:

* Die Abklingzeit einer Waffe beginnt nach dem Feuern.
* Eine Waffe kann erst wieder feuern, wenn ihre Abklingzeit abgelaufen ist.
* Wird ein Angriff während der Vorbereitungszeit abgebrochen, wird der Schuss abgebrochen.
* Bewegung unterbricht die Vorbereitungszeit von Waffen, bei denen die Einheit stillstehen muss.

---

## Projektile und Soforttrefferwaffen

* Soforttrefferwaffen bestimmen Genauigkeit und Schaden beim Feuern.
* Projektilwaffen erzeugen nach Abschluss der Vorbereitungszeit ein Projektil.
* Ein auf eine Einheit gerichtetes Projektil folgt diesem Ziel nur, wenn es als gelenkt definiert ist.
* Ein ungelenktes Projektil fliegt weiter zu der Position, an der sich das Ziel beim Feuern befand.
* Gelenkte Projektile werden zerstört, wenn ihr Ziel ungültig wird, sofern die Waffe keine neue Zielerfassung vorsieht.
* Ein Projektil, das sein Ziel erreicht, verarbeitet seinen Einschlag im aktuellen Simulationstick.
* Projektile dürfen nicht außerhalb der Kartengrenzen existieren.

---

## Flächenschaden und Eigenbeschuss

* Flächenschaden überprüft Ziele innerhalb des Einschlagsradius der Waffe.
* Direkt getroffene Ziele erhalten durch diesen Angriff nur eine Schadensinstanz.
* Flächenschaden verursacht Eigenbeschuss.

---

## Gleichzeitiger Schaden und überschüssiger Schaden

* Alle Einschläge, die im selben Simulationstick verarbeitet werden, verwenden den Zielzustand vom Beginn des Kampfverarbeitungsschritts.
* Mehrere Angreifer können sich daher im selben Tick gegenseitig zerstören.
* Schaden über die verbleibenden Trefferpunkte des Ziels hinaus ist überschüssiger Schaden und wird nicht auf ein anderes Ziel übertragen.
* Erfahrungspunkte werden anhand des wirksamen Schadens berechnet und schließen überschüssigen Schaden aus.

---

## Rüstungsklassen

Anfängliche Rüstungsklassen:

* Leicht
* Mittel
* Schwer
* Gebäude
* Luft
* Kapital

---

# Genauigkeitsregeln

Für den ersten Prototyp wird eine einfache Genauigkeitsberechnung verwendet.

```text
Trefferchance = Grundgenauigkeit - Bewegungsmalus + Veteranenbonus
```

Empfohlene Grenzen:

* Minimale Trefferchance: 40 %
* Maximale Trefferchance: 100 %

Anfängliche Regeln:

* Stillstehendes Ziel: kein Malus
* Bewegliches Ziel: -10 %
* Schnelles Ziel: -20 %
* Erfahrener Angreifer: +5 % bis +15 %

---

# Reichweitenregeln

Jede Waffe besitzt:

* Eine minimale Reichweite, falls zutreffend
* Eine maximale Reichweite
* Eine Sichtlinienanforderung
* Einschränkungen für Zieltypen

Artillerie- und Belagerungswaffen sollten Verteidigungsanlagen übertreffen, benötigen jedoch Schutz.

---

# Zielauswahlregeln

Einheiten priorisieren Ziele automatisch entsprechend ihrer Rolle.

Beispielprioritäten:

1. Aktuelles Befehlsziel
2. Unmittelbarer Angreifer
3. Bestes Konterziel in Reichweite
4. Nächstgelegenes gültiges Ziel

Spieler können die Zielauswahl mit direkten Angriffsbefehlen überschreiben.

Zusätzliche Zielauswahlregeln:

* Einheiten greifen Gegner außerhalb ihres Erfassungsradius nicht automatisch an.
* Ein direktes Angriffsziel bleibt bevorzugt, solange es gültig ist und sich innerhalb der Verfolgungsdistanz befindet.
* Wird ein Ziel verborgen, stellen direkt feuernde Einheiten den Angriff darauf ein.
* Bereits fliegende ungelenkte Projektile bewegen sich weiter zu ihrer zugewiesenen Einschlagsposition.
* Wird ein automatisch gewähltes Ziel ungültig, sucht die Einheit sofort nach einem anderen gültigen Ziel.
* Einheiten greifen neutrale Objekte automatisch an.

---

# Eroberungsregeln

Außenposten werden von Gesandteneinheiten erobert.

Eroberungsablauf:

* Die neutralen Verteidiger müssen zuerst besiegt werden.
* Die erobernde Einheit muss sich innerhalb des Eroberungsradius aufhalten.
* Die Eroberung dauert 30 Sekunden.

---

# Regeln für Außenpostenspezialisierungen

Verfügbare Spezialisierungen:

* Industrie
* Energie / Forschung
* Militär

Eine Spezialisierung sollte Ressourcen kosten und Zeit benötigen.

Grundregel:

* Kosten: 500, 1000, 1500 Materialien
* Zeit: 60 Sekunden

---

# Veteranenregeln

Einheiten sammeln durch ihre Teilnahme an Kämpfen Erfahrung.

Erfahrungsquellen:

* Schaden verursachen
* Einheiten zerstören
* Gebäude zerstören
* Außenposten erobern

Anfängliche Stufen:

| Stufe | Name         | Bonus                                      |
| ----: | ------------ | ------------------------------------------ |
|     1 | Regulär      | Kein Bonus                                 |
|     2 | Veteran      | +10 % Trefferpunkte, +5 % Genauigkeit      |
|     3 | Elite        | +15 % Schaden, +10 % Trefferpunkte         |
|     4 | Heroisch     | Schaltet Eigenschaft oder Fähigkeit frei   |

Kapitaleinheiten sollten anstelle eines Veteranenstatus ein Stufensystem besitzen.

---

# Nebel-des-Krieges-Regeln

Jeder Spieler besitzt drei Sichtbarkeitszustände:

* Unerforscht
* Erforscht
* Sichtbar

Regeln:

* Einheiten und Gebäude decken nahegelegenes Gelände auf.
* Gegnerische Einheiten sind nur in aktuell sichtbaren Bereichen sichtbar.
* Erforschte Bereiche zeigen das Gelände, aber keine aktuellen gegnerischen Bewegungen.
* Getarnte oder versteckte Einheiten erfordern Aufklärung.

Der Nebel des Krieges sollte die Aufklärung während des gesamten Spiels wichtig machen.

---

# Niederlageregeln

Ein Spieler ist besiegt, wenn er sämtliche kritische Kommandofähigkeit verliert.

Anfängliche Regel:

Ein Spieler verliert, wenn er Folgendes besitzt:

* Keine Kommandozentrale
* Keine Konstruktionseinheiten
* Keine Außenposten mit Wiederaufbaufähigkeit
* Keine Produktionsgebäude

Dies verhindert eine sofortige Niederlage allein durch den Verlust der Startbasis und verhindert gleichzeitig endloses Verstecken.

Wenn ein Spieler besiegt wird:

* Alle verbleibenden Einheiten dieses Spielers werden sofort zerstört.
* Alle verbleibenden Gebäude dieses Spielers werden inaktiv und können weder angreifen noch produzieren, forschen, aufdecken, reparieren oder Energie bereitstellen.
* Alle Produktions-, Bau-, Forschungs- und Befehlswarteschlangen werden abgebrochen.
* Der besiegte Spieler kann keine Spielbefehle mehr erteilen.
* In Teamspielen wird die Kontrolle über die Einheiten des besiegten Spielers nicht an Verbündete übertragen.
* Der besiegte Spieler darf als Beobachter im Spiel bleiben.

---

# Vermeidung von Patt-Situationen

Frontier Command sollte defensive Blockaden im späten Spiel vermeiden.

Systeme zur Vermeidung von Patt-Situationen:

## Wertvolle umkämpfte Außenposten

Zentrale und vorgeschobene Außenposten bieten starke strategische Vorteile.

## Belagerungswerkzeuge

Einheiten und Forschungen des späten Spiels schalten Werkzeuge zum Durchbrechen von Verteidigungsanlagen frei.

## Verteidigungsgrenzen

Verteidigungsanlagen sollten Angriffe verzögern, aber nicht dauerhaft aufhalten.

## Kapitaleinheiten

Kapitaleinheiten bieten Durchbruchspotenzial.

---

# Designziel

Die Spielregeln sollten ein Spiel erzeugen, in dem sich die Spieler ständig zwischen Wirtschaft, Expansion, Forschung und militärischer Stärke entscheiden müssen.

Ein Spieler sollte gewinnen, weil er bessere strategische Entscheidungen getroffen und entscheidende Aktionen auf dem Schlachtfeld ausgeführt hat, nicht weil er dauerhaft sicher hinter statischen Verteidigungsanlagen geblieben ist.
