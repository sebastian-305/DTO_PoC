using System.Text.Json.Nodes;
using ContractAnalysisBlueprintPoC.Blueprints;

namespace ContractAnalysisBlueprintPoC.Schema;

public static class SchemaBuilder
{
    public static JsonObject BuildResultSchema(AnalysisBlueprint blueprint)
    {
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = "object"
        };

        var required = new JsonArray
        {
            "summary",
            "sections",
            "conclusion"
        };

        var properties = new JsonObject();
        schema["properties"] = properties;
        schema["required"] = required;

        properties["summary"] = BuildSummarySchema(blueprint);
        properties["sections"] = BuildSectionsSchema(blueprint);
        properties["conclusion"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = blueprint.ConclusionLabel
        };

        if (!string.IsNullOrWhiteSpace(blueprint.CollectiveAgreementLabel))
        {
            properties["collectiveAgreement"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = blueprint.CollectiveAgreementLabel
            };
            required.Add("collectiveAgreement");
        }

        return schema;
    }

    private static JsonObject BuildSummarySchema(AnalysisBlueprint blueprint)
    {
        var summaryProperties = new JsonObject();
        var summaryRequired = new JsonArray();

        foreach (var field in blueprint.SummaryFields)
        {
            summaryProperties[field.Id] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = field.Description is not null
                    ? $"{field.Label} – {field.Description}"
                    : field.Label
            };
            summaryRequired.Add(field.Id);
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["description"] = blueprint.SummaryTitle,
            ["properties"] = summaryProperties,
            ["required"] = summaryRequired
        };
    }

    private static JsonObject BuildSectionsSchema(AnalysisBlueprint blueprint)
    {
        var sectionsProperties = new JsonObject();
        var sectionsRequired = new JsonArray();

        foreach (var section in blueprint.Sections)
        {
            sectionsProperties[section.Id] = BuildSectionSchema(section);
            sectionsRequired.Add(section.Id);
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["description"] = "Analyseabschnitte",
            ["properties"] = sectionsProperties,
            ["required"] = sectionsRequired
        };
    }

    private static JsonObject BuildSectionSchema(SectionBlueprint section)
    {
        var itemProperties = new JsonObject();
        var itemRequired = new JsonArray();

        foreach (var field in section.Fields)
        {
            itemProperties[field.Id] = field.Kind switch
            {
                SectionFieldKind.List => BuildListFieldSchema(field),
                _ => BuildStringFieldSchema(field)
            };
            itemRequired.Add(field.Id);
        }

        return new JsonObject
        {
            ["type"] = "array",
            ["description"] = section.Title,
            ["items"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = itemProperties,
                ["required"] = itemRequired
            }
        };
    }

    private static JsonObject BuildStringFieldSchema(SectionField field) => new()
    {
        ["type"] = "string",
        ["description"] = field.Label
    };

    private static JsonObject BuildListFieldSchema(SectionField field) => new()
    {
        ["type"] = "array",
        ["description"] = field.Label,
        ["items"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = $"{field.Label} (Eintrag)"
        }
    };
}
