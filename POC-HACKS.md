# PoC-Hacks — JOSYN.Jap.JAPServer

Diese Datei dokumentiert bewusst akzeptierte technische Schulden im PoC-Kontext.
**Einfach löschen**, sobald der JAPServer in eine produktive Infrastruktur überführt wird.

---

## 1. Fake-Implementierung (`JAPServer.cs`)

**Datei:** `JAPServer.cs`  
**Hack:** `FakeReadArgumentsFromFile()` liefert hartcodierte INI-Daten statt echter
Job-Argumente aus einer Datei oder Datenbank.  
**Produktiv-Ersatz:** Echte Argumentquelle anbinden (Datei, DB, Konfigurationsdienst).

## 2. Demo-Session-Key (`launchSettings.json`)

**Datei:** `JOSYN.Jap.JAPServer/Properties/launchSettings.json`  
**Hack:** Der Session-Key `dea5611d-d740-437f-ad93-7a5dc5ae4299` ist fest im
Launch-Profil eingetragen — nur für Entwicklungs- und Testzwecke geeignet.  
**Produktiv-Ersatz:** Session-Key zur Laufzeit generieren oder von einem
Orchestrierungsdienst übergeben lassen.

## 3. Hardcodierter Build-Ausgabepfad (`Directory.Build.props`)

**Datei:** `Directory.Build.props`  
**Hack:** Ausgabeverzeichnis ist auf `C:\Temp\VS.OUT\JOSYN\` festgelegt —
maschinenspezifisch, funktioniert nur auf dem Entwickler-Rechner.  
**Produktiv-Ersatz:** Entfernen oder durch einen relativen/CI-kompatiblen Pfad ersetzen
(Standard-MSBuild-Ausgabe ist ausreichend).
