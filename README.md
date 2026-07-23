# 🐴 Desktop Donkey (`D3SKT0P`)

Eine leichtgewichtige und charmante C# Windows-Forms-Anwendung, die einen animierten Pixel-Art-Esel direkt auf deinen Windows-Desktop bringt. Der Esel wandert eigenständig über deine Taskleiste, hält zum Verschnaufen an, passt seine Geschwindigkeit an den Bildschirmwänden an und reagiert dynamisch, wenn er per Drag & Drop gegriffen und durch die Gegend gezogen wird – inklusive interaktivem Sound!

---

## ✨ Features

* **Transparentes Overlay-Fenster:** Läuft rahmenlos und vollständig transparent (`LimeGreen` als Transparenz-Key) über allen Fenstern (`TopMost`), sauber platziert direkt über der Taskleiste.
* **Flüssige Zustandsmaschine & Animationen:** Vollständig animierte Zustände über eingebettete Ressourcen (`Resources`) für alle Richtungen und Verhaltensweisen:
  * Laufen & Gehen (`EselRunningToRight/Left`, `EselWalkingToLeft/Right`)
  * Ruhen & Atmen (`EselBreathingToLeft/Right`)
  * Greifen & Plaudern (`EselChattingToLeft/Right`)
  * Schwerkraft & Fallen (`EselFallingToLeft/Right`)
* **Interaktive Drag & Drop-Mechanik:** Packe den Esel mit der linken Maustaste, ziehe ihn über den Bildschirm, lausche der Sound-Schleife (`ia.wav`) und beobachte, wie er beim Loslassen wieder kontrolliert zu Boden fällt!
* **Autonomes Verhalten:** Zufällige Wander-Schleifen mit Pausen, Geschwindigkeitsdrosselung an den Bildschirmen und automatischem Richtungswechsel.
* **Optimiertes Rendering:** Integriertes Double Buffering (`OptimizedDoubleBuffer`) und `ImageAnimator`-Frame-Updates sorgen für eine flimmerfreie, flüssige Darstellung.

---

## 🛠️ Tech-Stack

* **Programmiersprache:** C# (.NET / Windows Forms)
* **UI-Framework:** WinForms (`System.Windows.Forms`)
* **Grafik / Audio:** `System.Drawing`, `System.Media.SoundPlayer`, eingebettete RESX-Ressourcen (`.gif`-Animationen & `.wav`-Audio)

---

## 🚀 Erste Schritte

### Voraussetzungen
* Windows-Betriebssystem
* .NET SDK / Visual Studio (mit installierter Workload für die .NET-Desktopentwicklung)

### Erstellen und Ausführen
1. Öffne das Projekt in Visual Studio.
2. Stelle sicher, dass alle Asset-Dateien (`.gif`-Animationen und `ia.wav`) korrekt in den **Eigenschaften/Ressourcen** des Projekts (`Resources.resx`) hinterlegt sind.
3. Erstelle die Projektmappe im Modus **Release** oder **Debug**.
4. Starte die ausführbare Datei (`D3SKT0P.exe`). Der Esel erscheint sofort und beginnt am unteren Bildschirmrand entlangzutraben!

---

## 🎮 Steuerung & Interaktion

| Aktion | Verhalten |
| :--- | :--- |
| **Leerlauf / Wandern** | Der Esel trottet oder geht eigenständig am unteren Rand deines Hauptbildschirms entlang. |
| **Linksklick & Ziehen** | Greift den Esel, aktiviert die Chat-Animation, ändert die Ausrichtung je nach Ziehrichtung und spielt den Sound in einer Schleife ab. |
| **Maustaste loslassen** | Lässt den Esel fallen, schaltet auf die Fall-Animation um und landet sicher am unteren Fensterrand (`groundLevel`). |
