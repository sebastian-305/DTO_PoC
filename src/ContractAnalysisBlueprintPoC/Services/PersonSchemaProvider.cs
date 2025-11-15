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
                ["description"] = "Voller Name der Person."
            },
            ["geburtsdatum"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Geburtsdatum im Format TT. Monat JJJJ (oder Jahr, falls unbekannt)."
            },
            ["geburtsort"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Ort und Land der Geburt."
            },
            ["nationalitaet"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Hauptsächliche Nationalität oder kulturelle Zugehörigkeit."
            },
            ["haupttaetigkeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurzbeschreibung des wichtigsten Tätigkeitsfeldes (z. B. Wissenschaftlerin, Musiker, Politikerin)."
            },
            ["bekannteWerke"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Liste prägender Werke, Projekte oder Leistungen mit Jahresangabe, falls möglich."
            },
            ["auszeichnungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Wichtige Auszeichnungen mit Jahr (falls bekannt)."
            },
            ["kurzbiografie"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Prägnante Zusammenfassung der wichtigsten Lebensstationen und Bedeutung der Person in 3–4 Sätzen."
            },
            ["bildPrompt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Bildbeschreibung, die die Person mit Namen, typischem Aussehen, Kleidungsstil und Stimmung schildert."
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
