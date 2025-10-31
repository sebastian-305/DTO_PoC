using System.Collections.Generic;
using ContractAnalysisBlueprintPoC.Blueprints;
using ContractAnalysisBlueprintPoC.Models;

namespace ContractAnalysisBlueprintPoC.Services;

public class SampleResultProvider
{
    private readonly AnalysisBlueprintRegistry _registry;

    public SampleResultProvider(AnalysisBlueprintRegistry registry)
    {
        _registry = registry;
    }

    public AnalysisResult GetSample(string blueprintId)
    {
        var blueprint = _registry.GetById(blueprintId);

        return blueprint.Id switch
        {
            "employment" => BuildEmploymentSample(blueprint),
            "rent" => BuildRentSample(blueprint),
            _ => throw new KeyNotFoundException($"Kein Sample für Blueprint '{blueprintId}' konfiguriert.")
        };
    }

    private static AnalysisResult BuildEmploymentSample(AnalysisBlueprint blueprint)
    {
        var summaryValues = new Dictionary<string, string>
        {
            ["arbeitgeber"] = "Acme GmbH",
            ["taetigkeit"] = "Softwareentwicklerin",
            ["beginn"] = "01.03.2024",
            ["dauer"] = "Unbefristet",
            ["gehalt_brutto"] = "€ 3.600,–",
            ["zuschlaege"] = "Keine Nachtzuschläge geregelt",
            ["arbeitsstunden"] = "38,5 Stunden/Woche",
            ["ueberstunden"] = "Abgeltung durch All-In-Klausel",
            ["probezeit"] = "1 Monat"
        };

        var sections = new Dictionary<string, List<Dictionary<string, object?>>>
        {
            ["unzulaessige_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "Überlange Bindungsfrist",
                    ["zitat"] = "Die Arbeitnehmerin verpflichtet sich für 5 Jahre an das Unternehmen gebunden zu bleiben.",
                    ["problem"] = "Bindungsfristen über 3 Jahre sind bei fehlenden Ausbildungen unzulässig.",
                    ["rechtsgrundlage"] = new List<string>
                    {
                        "§ 2d AVRAG – Ausbildungskostenrückersatz",
                        "OGH 9 ObA 123/21f"
                    }
                }
            },
            ["nachteile_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "All-In ohne Überstundenaufschlüsselung",
                    ["zitat"] = "Das vereinbarte Entgelt deckt sämtliche Mehr- und Überstunden ab.",
                    ["problem"] = "Es fehlt eine nachvollziehbare Gegenüberstellung von Grundlohn und Pauschale.",
                    ["rechtsgrundlage"] = new List<string>
                    {
                        "§ 2 Abs. 2a AVRAG – Aliquotierung von All-In-Bezügen"
                    }
                }
            },
            ["widerspruechliche_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "Gleitzeit vs. fixe Arbeitszeiten",
                    ["zitat_klausel_a"] = "Die Arbeitnehmerin arbeitet im Gleitzeitmodell.",
                    ["zitat_klausel_b"] = "Dienstbeginn ist täglich um 8:00 Uhr festgelegt.",
                    ["problem"] = "Fixe Beginnzeiten widersprechen der vereinbarten Gleitzeitregelung.",
                    ["rechtsgrundlage"] = new List<string>
                    {
                        "§ 4b AZG – Gleitzeitvereinbarungen"
                    }
                }
            },
            ["vorteilhafte_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "Weiterbildungsbudget",
                    ["zitat"] = "Die Arbeitgeberin stellt jährlich € 1.000 für Fortbildungen zur Verfügung.",
                    ["vorteil"] = "Unterstützt persönliche Entwicklung und kann steuerfrei genutzt werden."
                }
            }
        };

        return new AnalysisResult
        {
            BlueprintId = blueprint.Id,
            Summary = CreateSummaryDictionary(blueprint, summaryValues),
            Sections = EnsureAllSectionKeys(blueprint, sections),
            Conclusion = "Der Vertrag enthält einzelne problematische Klauseln, sollte aber nachverhandelt werden.",
            CollectiveAgreement = "Kollektivvertrag Handel 2024"
        };
    }

    private static AnalysisResult BuildRentSample(AnalysisBlueprint blueprint)
    {
        var summaryValues = new Dictionary<string, string>
        {
            ["vermieter"] = "Wohnbau GmbH",
            ["mieter"] = "Max Mustermann",
            ["beginn"] = "01.05.2024",
            ["dauer"] = "Unbefristet",
            ["mindestdauer"] = "12 Monate",
            ["kuendigungsfrist"] = "3 Monate",
            ["miete_kalt"] = "€ 820,–",
            ["betriebskosten"] = "€ 180,–",
            ["quadratmeter"] = "65 m²",
            ["zimmer"] = "3 Zimmer",
            ["im_mietgegenstand_enthalten"] = "Küche, Einbauschränke, Tiefgaragenplatz"
        };

        var sections = new Dictionary<string, List<Dictionary<string, object?>>>
        {
            ["unzulaessige_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "Unzulässige Pauschalierung von Schäden",
                    ["zitat"] = "Der Mieter haftet pauschal mit € 3.000 für sämtliche Schäden.",
                    ["problem"] = "Pauschale Schadenersatzsummen ohne Nachweis sind unwirksam.",
                    ["rechtsgrundlage"] = new List<string>
                    {
                        "§ 879 Abs. 3 ABGB – gröbliche Benachteiligung"
                    }
                }
            },
            ["nachteile_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "Indexierung ohne Obergrenze",
                    ["zitat"] = "Die Miete wird halbjährlich nach VPI angepasst.",
                    ["problem"] = "Fehlende Obergrenze führt zu schwer kalkulierbaren Mehrkosten.",
                    ["rechtsgrundlage"] = new List<string>
                    {
                        "§ 6 Abs. 1 Z 5 KSchG – unbestimmte Preisänderungen"
                    }
                }
            },
            ["widerspruechliche_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "Tierhaltung",
                    ["zitat_klausel_a"] = "Haustiere sind grundsätzlich erlaubt.",
                    ["zitat_klausel_b"] = "Die Haltung jeglicher Tiere bedarf der vorherigen Zustimmung des Vermieters.",
                    ["problem"] = "Allgemeine Erlaubnis und Zustimmungspflicht widersprechen einander.",
                    ["rechtsgrundlage"] = new List<string>
                    {
                        "OGH 7 Ob 143/12p – Tierhaltung in Mietwohnungen"
                    }
                }
            },
            ["vorteilhafte_klauseln"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["titel"] = "Staffelrabatt bei langer Mietdauer",
                    ["zitat"] = "Nach drei Jahren reduziert sich die Miete um 5 %.",
                    ["vorteil"] = "Belohnt langfristige Mietverhältnisse mit spürbarer Ersparnis."
                }
            }
        };

        return new AnalysisResult
        {
            BlueprintId = blueprint.Id,
            Summary = CreateSummaryDictionary(blueprint, summaryValues),
            Sections = EnsureAllSectionKeys(blueprint, sections),
            Conclusion = "Der Mietvertrag ist grundsätzlich fair, weist aber einzelne Nachteile auf.",
            CollectiveAgreement = null
        };
    }

    private static Dictionary<string, string> CreateSummaryDictionary(
        AnalysisBlueprint blueprint,
        IDictionary<string, string> values)
    {
        var result = new Dictionary<string, string>();

        foreach (var field in blueprint.SummaryFields)
        {
            if (values.TryGetValue(field.Id, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                result[field.Id] = value;
            }
            else
            {
                result[field.Id] = "–";
            }
        }

        return result;
    }

    private static Dictionary<string, List<Dictionary<string, object?>>> EnsureAllSectionKeys(
        AnalysisBlueprint blueprint,
        Dictionary<string, List<Dictionary<string, object?>>> sections)
    {
        foreach (var section in blueprint.Sections)
        {
            if (!sections.ContainsKey(section.Id))
            {
                sections[section.Id] = new List<Dictionary<string, object?>>();
            }
        }

        return sections;
    }
}
