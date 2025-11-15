using System.Text.Json.Nodes;

namespace ContractAnalysisBlueprintPoC.Services;

public sealed class PersonSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Voller Name der Person.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Person",
                    ["order"] = 10,
                    ["variant"] = "highlight",
                    ["hint"] = "Vor- und Nachname bzw. Künstlername.",
                    ["tooltip"] = "Voller Name oder Künstlername, wie er im Text erscheinen soll."
                }
            },
            ["geburtsdatum"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Geburtsdatum im Format TT. Monat JJJJ (oder Jahr, falls unbekannt).",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Geburtsdatum",
                    ["order"] = 20,
                    ["variant"] = "stat",
                    ["hint"] = "Datum vollständig nennen, alternativ Jahr.",
                    ["tooltip"] = "Geburtsdatum im Format Tag. Monat Jahr oder zumindest das Jahr."
                }
            },
            ["geburtsort"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Ort und Land der Geburt.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Geburtsort",
                    ["order"] = 30,
                    ["hint"] = "Bitte Stadt und Land kombinieren.",
                    ["tooltip"] = "Ort und Land, an dem die Person geboren wurde."
                }
            },
            ["nationalitaet"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Hauptsächliche Nationalität oder kulturelle Zugehörigkeit.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Nationalität",
                    ["order"] = 40,
                    ["variant"] = "highlight",
                    ["hint"] = "Wichtigste Staatsangehörigkeit oder kulturelle Zugehörigkeit.",
                    ["tooltip"] = "Primäre Staatsangehörigkeit oder kulturelle Zugehörigkeit."
                }
            },
            ["haupttaetigkeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurzbeschreibung des wichtigsten Tätigkeitsfeldes (z. B. Wissenschaftlerin, Musiker, Politikerin).",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Haupttätigkeit",
                    ["order"] = 50,
                    ["hint"] = "Rolle oder Berufsbezeichnung prägnant beschreiben.",
                    ["tooltip"] = "Kernberuf bzw. Rolle, für die die Person bekannt ist."
                }
            },
            ["bekannteWerke"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Liste prägender Werke, Projekte oder Leistungen mit Jahresangabe, falls möglich.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Bekannte Werke",
                    ["order"] = 60,
                    ["variant"] = "pill-list",
                    ["hint"] = "Wichtige Werke oder Leistungen als Liste, gerne inklusive Jahr.",
                    ["tooltip"] = "Liste prägender Werke, Projekte oder Leistungen mit Jahresangabe."
                }
            },
            ["auszeichnungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Wichtige Auszeichnungen mit Jahr (falls bekannt).",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Auszeichnungen",
                    ["order"] = 70,
                    ["variant"] = "pill-list",
                    ["hint"] = "Relevante Preise oder Ehrungen inkl. Jahr, wenn möglich.",
                    ["tooltip"] = "Wichtige Preise oder Ehrungen samt Jahr."
                }
            },
            ["kurzbiografie"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Prägnante Zusammenfassung der wichtigsten Lebensstationen und Bedeutung der Person in 3–4 Sätzen.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Kurzbiografie",
                    ["order"] = 80,
                    ["hint"] = "In wenigen Sätzen Lebenslauf und Bedeutung skizzieren.",
                    ["tooltip"] = "Kurze Zusammenfassung der wichtigsten Stationen und Bedeutung."
                }
            },
            ["bildPrompt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Bildbeschreibung, die die Person mit Namen, typischem Aussehen, Kleidungsstil und Stimmung schildert.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Bildidee",
                    ["order"] = 90,
                    ["variant"] = "muted",
                    ["hint"] = "Prompt mit Namen, Erscheinungsbild, Outfit und Stimmung für die Bild-KI.",
                    ["tooltip"] = "Prompt, der Name, Aussehen, Kleidung und Stimmung für die Bild-KI beschreibt."
                }
            }
        },
        ["required"] = new JsonArray(
            "name",
            "geburtsdatum",
            "nationalitaet",
            "haupttaetigkeit",
            "bekannteWerke",
            "kurzbiografie",
            "bildPrompt")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
