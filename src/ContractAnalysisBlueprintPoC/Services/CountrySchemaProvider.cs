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
            }
        },
        ["required"] = new JsonArray("hauptstadt", "einwohner", "flaeche", "sprachen", "kontinent")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
