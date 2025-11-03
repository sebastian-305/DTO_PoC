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
            ["hauptstadt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Die Hauptstadt des Landes."
            },
            ["einwohner"] = new JsonObject
            {
                ["type"] = "object",
                ["description"] = "Aktuelle Einwohnerzahl inklusive erläuterndem Hinweis.",
                ["additionalProperties"] = false,
                ["properties"] = new JsonObject
                {
                    ["anzahl"] = new JsonObject
                    {
                        ["type"] = "number",
                        ["description"] = "Geschätzte Einwohnerzahl (Zahl)."
                    },
                    ["hinweis"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Kurzer Kontext, z. B. Quelle oder Stand."
                    }
                },
                ["required"] = new JsonArray("anzahl")
            },
            ["flaeche"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Fläche des Landes inklusive Maßeinheit."
            },
            ["sprachen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Wichtige Amtssprachen und verbreitete Sprachen."
            },
            ["kontinent"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kontinent, auf dem das Land liegt."
            },
            ["bildPrompt"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Prägnanter Prompt für eine bildliche Darstellung (Motiv, Stil, Lichtstimmung)."
            }
        },
        ["required"] = new JsonArray("hauptstadt", "einwohner", "flaeche", "sprachen", "kontinent", "bildPrompt")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
