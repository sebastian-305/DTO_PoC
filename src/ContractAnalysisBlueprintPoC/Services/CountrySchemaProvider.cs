using System.Text.Json.Nodes;

namespace ContractAnalysisBlueprintPoC.Services;

public sealed class CountrySchemaProvider
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
                ["description"] = "Offizieller oder gebräuchlicher Name des Landes.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Land",
                    ["order"] = 10,
                    ["variant"] = "highlight",
                    ["hint"] = "Offizieller oder gebräuchlicher Name bzw. Eigenbezeichnung.",
                    ["tooltip"] = "Name des Landes, so wie er üblich verwendet wird."
                }
            },
            ["hauptstadt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name der Hauptstadt.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Hauptstadt",
                    ["order"] = 20,
                    ["variant"] = "highlight",
                    ["hint"] = "Politisches Zentrum oder Regierungssitz.",
                    ["tooltip"] = "Name der Hauptstadt beziehungsweise des Regierungssitzes."
                }
            },
            ["einwohnerzahl"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Aktuelle oder zuletzt verfügbare Einwohnerzahl mit Jahr.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Einwohnerzahl",
                    ["order"] = 30,
                    ["variant"] = "stat",
                    ["hint"] = "Bitte die Zahl zusammen mit dem Referenzjahr nennen.",
                    ["tooltip"] = "Aktuelle oder letzte bekannte Bevölkerungszahl inklusive Jahr."
                }
            },
            ["flaeche"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Fläche in Quadratkilometern mit optionaler Quelle.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Fläche",
                    ["order"] = 40,
                    ["variant"] = "stat",
                    ["hint"] = "Bitte gerundete Fläche in km² angeben.",
                    ["tooltip"] = "Fläche des Landes in Quadratkilometern."
                }
            },
            ["amtssprachen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Liste der Amtssprachen.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Amtssprachen",
                    ["order"] = 50,
                    ["variant"] = "pill-list",
                    ["hint"] = "Mehrere Sprachen bitte als Liste ausgeben.",
                    ["tooltip"] = "Liste der offiziellen Amtssprachen."
                }
            },
            ["kontinent"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kontinent, auf dem das Land liegt.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Kontinent",
                    ["order"] = 60,
                    ["tooltip"] = "Kontinent, auf dem das Land überwiegend liegt."
                }
            },
            ["staatsform"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Aktuelle Staats- oder Regierungsform.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Staatsform",
                    ["order"] = 70,
                    ["hint"] = "Z. B. parlamentarische Republik oder konstitutionelle Monarchie.",
                    ["tooltip"] = "Beschreibt die Staats- oder Regierungsform."
                }
            },
            ["kurzbeschreibung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zusammenfassung der geografischen, wirtschaftlichen oder kulturellen Besonderheiten in 3–4 Sätzen.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Kurzbeschreibung",
                    ["order"] = 80,
                    ["hint"] = "Knapp die wichtigsten Besonderheiten hervorheben.",
                    ["tooltip"] = "Kurze Zusammenfassung der wichtigsten Merkmale des Landes."
                }
            },
            ["bildPrompt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Szenische Beschreibung eines typischen Motivs, das das Land visuell repräsentiert.",
                ["x-ui"] = new JsonObject
                {
                    ["label"] = "Bildidee",
                    ["order"] = 90,
                    ["variant"] = "muted",
                    ["hint"] = "Knackiger Prompt, der der Bild-KI als Vorlage dient.",
                    ["tooltip"] = "Prompt, der ein typisches Motiv des Landes für die Bild-KI beschreibt."
                }
            }
        },
        ["required"] = new JsonArray(
            "name",
            "hauptstadt",
            "einwohnerzahl",
            "flaeche",
            "amtssprachen",
            "kontinent",
            "kurzbeschreibung",
            "bildPrompt")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
