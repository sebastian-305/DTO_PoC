# ContractAnalysisBlueprintPoC

ContractAnalysisBlueprintPoC demonstriert, wie ein gemeinsamer Analyse-Blueprint gleichzeitig das Backend, die JSON-Schema-Generierung sowie das Frontend eines Vertrags-Analyse-Tools versorgt. Arbeits- und Mietverträge greifen auf dieselbe Struktur zu, sodass Änderungen an Feldern nur noch an einer Stelle gepflegt werden müssen.

## Projektaufbau

- **Backend (.NET 8 Minimal API)** liefert Blueprint-Metadaten, JSON-Schemata und Beispielergebnisse aus einer Registry.
- **SchemaBuilder** generiert JSON-Schemata gemäß Draft 2020-12 direkt aus dem Blueprint.
- **Frontend (Vanilla JS)** rendert Summary, Abschnittskarten und Fazit dynamisch anhand der Blueprint-Definition und der gelieferten Beispieldaten.
- **Tests (xUnit)** prüfen die Schema-Generierung.

## Starten der Anwendung

1. Abhängigkeiten wiederherstellen und Web-App starten:
   ```bash
   dotnet run --project src/ContractAnalysisBlueprintPoC/ContractAnalysisBlueprintPoC.csproj
   ```
2. Die Oberfläche ist anschließend unter `http://localhost:5000` (oder dem in der Konsole angezeigten Port) erreichbar.
3. Über das Dropdown lässt sich zwischen Arbeits- und Mietvertrag wechseln. Buttons laden Beispieldaten neu oder zeigen das Schema des aktuellen Blueprints an.

## Tests ausführen

```bash
dotnet test
```

## Funktionsweise des SchemaBuilder

`SchemaBuilder.BuildResultSchema` baut das Ergebnis-Schema ausschließlich aus den Blueprint-Daten auf. Summary-Felder und Abschnittsstrukturen werden mitsamt Typen, Beschreibungen und Required-Listen übernommen. Dadurch bleibt Schema, DTO und Frontend automatisch konsistent, sobald der Blueprint erweitert wird.
