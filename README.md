# Länderanalyse mit Nebius

Diese Anwendung demonstriert eine minimalistische End-to-End-Integration zwischen einer .NET-Minimal-API, einem schlanken JavaScript-Frontend und der Nebius-API für Structured Outputs. Nutzer:innen geben lediglich den Namen eines Landes ein und erhalten strukturierte Informationen zur Hauptstadt, Einwohnerzahl, Fläche, Sprachen und zum Kontinent zurück.

## Architekturüberblick

- **Backend (.NET 8 Minimal API)**
  - Endpunkt `POST /api/analyze` nimmt den Ländernamen entgegen und beauftragt die Nebius-API mit einer Analyse.
  - Endpunkt `GET /api/schema` stellt das aktuell verwendete JSON-Schema bereit.
  - Das Schema lässt sich zentral in `CountrySchemaProvider` anpassen und wird für den Structured-Output-Call verwendet.
- **Frontend (Vanilla JS)**
  - Ein Textfeld und der Button „Analyse erzeugen“ senden den Ländernamen an das Backend.
  - Die Antwort wird formatiert als JSON angezeigt.

## Projekt lokal starten

1. API-Key und Modell der Nebius-API als Umgebungsvariablen setzen (oder in `appsettings.json` hinterlegen):
   ```bash
   export Nebius__ApiKey="<dein-api-key>"
   export Nebius__Model="<dein-modell>"
   ```
2. Anwendung aus dem Projektverzeichnis starten:
   ```bash
   dotnet run --project src/ContractAnalysisBlueprintPoC/ContractAnalysisBlueprintPoC.csproj
   ```
3. Das Frontend ist anschließend unter `http://localhost:5000` (oder dem ausgegebenen Port) erreichbar.

## Tests ausführen

```bash
dotnet test
```

## Schema anpassen

Das verwendete Structured-Output-Schema liegt in `CountrySchemaProvider`. Änderungen an dieser Klasse werden automatisch beim nächsten API-Aufruf berücksichtigt. So lassen sich zusätzliche Felder hinzufügen oder bestehende Beschreibungen anpassen, ohne das Frontend ändern zu müssen.
