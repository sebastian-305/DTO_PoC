using System.Linq;
using System.Text.Json.Nodes;
using ContractAnalysisBlueprintPoC.Blueprints;
using ContractAnalysisBlueprintPoC.Schema;
using Xunit;

namespace ContractAnalysisBlueprintPoC.Tests;

public class SchemaBuilderTests
{
    private readonly AnalysisBlueprintRegistry _registry = new();

    [Fact]
    public void EmploymentSchemaContainsAllSummaryFields()
    {
        var blueprint = _registry.GetById("employment");
        var schema = SchemaBuilder.BuildResultSchema(blueprint);

        var summary = schema["properties"]?.AsObject()["summary"]?.AsObject();
        Assert.NotNull(summary);

        var summaryProperties = summary!["properties"]?.AsObject();
        Assert.NotNull(summaryProperties);

        foreach (var field in blueprint.SummaryFields)
        {
            Assert.True(summaryProperties!.ContainsKey(field.Id));
        }

        var required = summary["required"]?.AsArray();
        Assert.NotNull(required);

        var requiredIds = required!.Select(node => node!.GetValue<string>()).ToList();
        Assert.True(blueprint.SummaryFields.All(field => requiredIds.Contains(field.Id)));
    }

    [Fact]
    public void ContradictoryClauseSectionProducesExpectedFieldTypes()
    {
        var blueprint = _registry.GetById("employment");
        var schema = SchemaBuilder.BuildResultSchema(blueprint);

        var sectionItems = schema["properties"]?.AsObject()["sections"]?.AsObject()["properties"]?
            .AsObject()["widerspruechliche_klauseln"]?.AsObject()["items"]?.AsObject();

        Assert.NotNull(sectionItems);

        var properties = sectionItems!["properties"]?.AsObject();
        Assert.NotNull(properties);

        var emphasisField = properties!["zitat_klausel_b"]?.AsObject();
        Assert.NotNull(emphasisField);
        Assert.Equal("string", emphasisField!["type"]?.GetValue<string>());

        var listField = properties["rechtsgrundlage"]?.AsObject();
        Assert.NotNull(listField);
        Assert.Equal("array", listField!["type"]?.GetValue<string>());

        var items = listField["items"]?.AsObject();
        Assert.NotNull(items);
        Assert.Equal("string", items!["type"]?.GetValue<string>());
    }

    [Fact]
    public void RentSchemaDoesNotRequireCollectiveAgreement()
    {
        var blueprint = _registry.GetById("rent");
        var schema = SchemaBuilder.BuildResultSchema(blueprint);

        var required = schema["required"]?.AsArray();
        Assert.NotNull(required);

        var requiredIds = required!.Select(node => node!.GetValue<string>()).ToList();
        Assert.DoesNotContain("collectiveAgreement", requiredIds);
    }
}
