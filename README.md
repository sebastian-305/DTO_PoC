# Analyse-Assistent mit Nebius

Diese Anwendung demonstriert eine minimalistische End-to-End-Integration zwischen einer .NET-Minimal-API, einem schlanken JavaScript-Frontend und der Nebius-API für Structured Outputs. Nutzer:innen können wahlweise den Namen einer berühmten Person oder eines Landes angeben und erhalten strukturierte Informationen inklusive einer Bildidee.

## Architekturüberblick

- **Backend (.NET 8 Minimal API)**
  - Endpunkt `POST /api/analyze` nimmt den gewünschten Analysetyp (`person` oder `country`) entgegen und beauftragt die Nebius-API mit einer entsprechenden Analyse.
  - Endpunkt `GET /api/schema` stellt das aktuell verwendete JSON-Schema bereit und akzeptiert optional den Query-Parameter `type`.
  - Die Schemata lassen sich zentral in `PersonSchemaProvider` und `CountrySchemaProvider` anpassen und werden für den Structured-Output-Call verwendet.
- **Frontend (Vanilla JS)**
  - Eine Umschaltung erlaubt die Auswahl zwischen Personen- und Länderanalyse, anschließend wird der Suchbegriff an das Backend übermittelt.
  - Die Antwort wird formatiert als JSON angezeigt und bei vorhandenen Bild-Prompts um eine Bildgenerierung ergänzt.

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

Die verwendeten Structured-Output-Schemata liegen in `PersonSchemaProvider` und `CountrySchemaProvider`. Änderungen an diesen Klassen werden automatisch beim nächsten API-Aufruf berücksichtigt. So lassen sich zusätzliche Felder hinzufügen oder bestehende Beschreibungen anpassen, ohne das Frontend ändern zu müssen.
