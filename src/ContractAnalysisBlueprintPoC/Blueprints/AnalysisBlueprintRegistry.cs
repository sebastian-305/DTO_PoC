using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractAnalysisBlueprintPoC.Blueprints;

public class AnalysisBlueprintRegistry
{
    private readonly IReadOnlyList<AnalysisBlueprint> _blueprints;

    public AnalysisBlueprintRegistry()
    {
        _blueprints = new List<AnalysisBlueprint>
        {
            BuildEmploymentBlueprint(),
            BuildRentBlueprint()
        };
    }

    public IReadOnlyList<AnalysisBlueprint> GetAll() => _blueprints;

    public AnalysisBlueprint GetById(string id) =>
        _blueprints.FirstOrDefault(bp => string.Equals(bp.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Blueprint '{id}' wurde nicht gefunden.");

    private static AnalysisBlueprint BuildEmploymentBlueprint()
    {
        var summaryFields = new List<SummaryField>
        {
            new("arbeitgeber", "Arbeitgeber"),
            new("taetigkeit", "Tätigkeit"),
            new("beginn", "Beginn"),
            new("dauer", "Dauer"),
            new("gehalt_brutto", "Bruttogehalt"),
            new("zuschlaege", "Zuschläge"),
            new("arbeitsstunden", "Arbeitsstunden"),
            new("ueberstunden", "Überstunden"),
            new("probezeit", "Probezeit")
        };

        return new AnalysisBlueprint(
            "employment",
            "Arbeitsvertrag",
            "Zusammenfassung",
            summaryFields,
            BuildSharedSections(),
            "Fazit",
            "Kollektivvertrag");
    }

    private static AnalysisBlueprint BuildRentBlueprint()
    {
        var summaryFields = new List<SummaryField>
        {
            new("vermieter", "Vermieter"),
            new("mieter", "Mieter"),
            new("beginn", "Beginn"),
            new("dauer", "Dauer"),
            new("mindestdauer", "Mindestdauer"),
            new("kuendigungsfrist", "Kündigungsfrist"),
            new("miete_kalt", "Kaltmiete"),
            new("betriebskosten", "Betriebskosten"),
            new("quadratmeter", "Quadratmeter"),
            new("zimmer", "Zimmer"),
            new("im_mietgegenstand_enthalten", "Im Mietgegenstand enthalten")
        };

        return new AnalysisBlueprint(
            "rent",
            "Mietvertrag",
            "Zusammenfassung",
            summaryFields,
            BuildSharedSections(),
            "Bewertung");
    }

    private static IReadOnlyList<SectionBlueprint> BuildSharedSections()
    {
        return new List<SectionBlueprint>
        {
            new(
                "unzulaessige_klauseln",
                "❗ Unzulässige Klauseln",
                "ban",
                new List<SectionField>
                {
                    new("titel", "Titel"),
                    new("zitat", "Zitat"),
                    new("problem", "Problem"),
                    new("rechtsgrundlage", "Rechtsgrundlage", SectionFieldKind.List)
                }),
            new(
                "nachteile_klauseln",
                "⚠️ Nachteilige Klauseln",
                "triangle-exclamation",
                new List<SectionField>
                {
                    new("titel", "Titel"),
                    new("zitat", "Zitat"),
                    new("problem", "Problem"),
                    new("rechtsgrundlage", "Rechtsgrundlage", SectionFieldKind.List)
                }),
            new(
                "widerspruechliche_klauseln",
                "🟠 Widersprüchliche Klauseln",
                "code-compare",
                new List<SectionField>
                {
                    new("titel", "Titel"),
                    new("zitat_klausel_a", "Zitat Klausel A"),
                    new("zitat_klausel_b", "Zitat Klausel B", SectionFieldKind.Emphasis),
                    new("problem", "Problem"),
                    new("rechtsgrundlage", "Rechtsgrundlage", SectionFieldKind.List)
                }),
            new(
                "vorteilhafte_klauseln",
                "✅ Vorteilhafte Klauseln",
                "circle-check",
                new List<SectionField>
                {
                    new("titel", "Titel"),
                    new("zitat", "Zitat"),
                    new("vorteil", "Vorteil")
                })
        };
    }
}
