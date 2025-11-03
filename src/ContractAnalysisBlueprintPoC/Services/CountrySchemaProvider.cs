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
                ["description"] = "Offizieller oder gebräuchlicher Name des Landes."
            },
            ["hauptstadt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name der Hauptstadt."
            },
            ["einwohnerzahl"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Aktuelle oder zuletzt verfügbare Einwohnerzahl mit Jahr."
            },
            ["flaeche"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Fläche in Quadratkilometern mit optionaler Quelle."
            },
            ["amtssprachen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Liste der Amtssprachen."
            },
            ["kontinent"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kontinent, auf dem das Land liegt."
            },
            ["staatsform"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Aktuelle Staats- oder Regierungsform."
            },
            ["kurzbeschreibung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zusammenfassung der geografischen, wirtschaftlichen oder kulturellen Besonderheiten in 3–4 Sätzen."
            },
            ["bildPrompt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Szenische Beschreibung eines typischen Motivs, das das Land visuell repräsentiert."
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
