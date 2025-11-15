# Analyse-Assistent mit Nebius

Diese Anwendung demonstriert eine vollständige End-to-End-Integration zwischen einer .NET-8-Minimal-API, einem Vanilla-JS-Frontend und der Nebius-API für Structured Outputs **inklusive automatischer Bildgenerierung**. Nutzer:innen können wahlweise den Namen einer berühmten Person oder eines Landes angeben. Das Backend ruft das passende JSON-Schema ab, fragt Nebius an und liefert strukturierte Daten samt Prompt und – falls möglich – ein durch die Bild-API erzeugtes Vorschaubild.

## Architekturüberblick

- **Backend (.NET 8 Minimal API)**
  - Endpunkt `POST /api/analyze` nimmt den gewünschten Analysetyp (`person` oder `country`) entgegen, prüft Eingaben und ruft das passende JSON-Schema aus `PersonSchemaProvider` bzw. `CountrySchemaProvider` ab.
  - Structured-Output-Calls laufen über `NebiusService`, der die OpenAI .NET SDKs gegen die Nebius-Endpunkte nutzt. Bild-Prompts werden zusätzlich an `NebiusImageService` weitergeleitet, um via Image-API ein Preview zu erzeugen.
  - Fehler der Nebius-API werden aufgefangen und als ProblemDetails inklusive Original-Statuscode an den Client zurückgegeben.
- **Frontend (Vanilla JS + CSS)**
  - Ein Umschalter erlaubt die Auswahl zwischen Personen- und Länderanalyse. Passend zur Auswahl werden Platzhalter, Feldbeschreibungen und Schema-Hinweise aktualisiert.
  - Das Ergebnis wird als formatiertes JSON-ähnliches Layout dargestellt. Zusätzliche Metadaten aus dem Schema (`x-ui`) bestimmen Reihenfolge, Labels und Varianten (Highlight, Stat, Pill-Listen usw.).
  - Liegt ein `bildPrompt` vor, zeigt das Frontend den Prompt an und bindet – sofern vorhanden – das generierte Bild oder den Fehlertext ein.

## Projekt lokal starten

1. **Nebius-Zugangsdaten setzen** – entweder per Umgebungsvariablen oder über `appsettings.json`:
   ```bash
   export Nebius__ApiKey="<dein-api-key>"
   export Nebius__Model="meta-llama/Meta-Llama-3.1-8B-Instruct-fast"
   export Nebius__ImageModel="black-forest-labs/flux-schnell"   # optional, sonst wird Model genutzt
   export Nebius__Endpoint="https://api.studio.nebius.com/v1/"
   ```
   > Hinweis: Die Defaults findest du in `src/ContractAnalysisBlueprintPoC/appsettings.json`.
2. **Anwendung starten**:
   ```bash
   dotnet run --project src/ContractAnalysisBlueprintPoC/ContractAnalysisBlueprintPoC.csproj
   ```
3. **Frontend öffnen** – standardmäßig unter `http://localhost:5000` (oder dem von ASP.NET Core ausgegebenen Port).

## Tests ausführen

Die Solution enthält Komponententests für die Schema-Provider sowie Endpoint-Tests, die das Fehlerverhalten gegenüber Nebius simulieren. Ausführen kannst du sie mit:

```bash
dotnet test
```

## Schema & Darstellung anpassen

- Die Structured-Output-Schemata inkl. UI-Metadaten liegen in `PersonSchemaProvider` und `CountrySchemaProvider`. Änderungen werden beim nächsten Request automatisch verwendet.
- Das Frontend liest die `x-ui`-Metadaten aus dem Schema (`/api/schema`) und entscheidet damit über Reihenfolge, Beschriftung, Stil und Tooltips der einzelnen Felder.
- Zusätzliche Felder lassen sich dadurch ohne Änderungen am Frontend einführen – solange sie im Schema beschrieben sind, erscheinen sie automatisch in der Ausgabe.

## Weiterführende Hinweise

- `NebiusService` kapselt alle Structured-Output-Aufrufe und setzt u. a. ein Timeout sowie Logging der Token-Nutzung.
- `NebiusImageService` kann optional deaktiviert werden (z. B. per DI), die API liefert dann lediglich den Prompt.
- Für produktive Deployments empfiehlt es sich, die sensiblen Einstellungen ausschließlich über Secret Stores oder Key Vaults zu setzen.
