using System.Linq;
using System.Text.Json.Nodes;
using ContractAnalysisBlueprintPoC.Services;
using FluentAssertions;
using Xunit;

namespace ContractAnalysisBlueprintPoC.Tests;

public class PersonSchemaProviderTests
{
    private readonly PersonSchemaProvider _provider = new();

    [Fact]
    public void GetSchema_ShouldReturnClone()
    {
        var first = _provider.GetSchema();
        var second = _provider.GetSchema();

        second["properties"]!["name"]!["description"] = "verändert";

        first["properties"]!["name"]!["description"]!
            .GetValue<string>()
            .Should().NotBe("verändert");
    }

    [Fact]
    public void Schema_ShouldExposeMetadataLikeCountrySchema()
    {
        var schema = _provider.GetSchema();

        schema["type"]!.GetValue<string>().Should().Be("object");
        schema["additionalProperties"]!.GetValue<bool>().Should().BeFalse();

        var required = schema["required"]!.AsArray().Select(node => node!.GetValue<string>()).ToArray();
        required.Should().BeEquivalentTo(new[]
        {
            "name",
            "geburtsdatum",
            "nationalitaet",
            "haupttaetigkeit",
            "bekannteWerke",
            "kurzbiografie",
            "bildPrompt"
        });

        var properties = schema["properties"]!.AsObject();
        properties.Should().ContainKey("geburtsort");

        var nameUi = GetUi(properties, "name");
        nameUi["label"]!.GetValue<string>().Should().Be("Person");
        nameUi["order"]!.GetValue<int>().Should().Be(10);
        nameUi["variant"]!.GetValue<string>().Should().Be("highlight");
        nameUi["tooltip"]!.GetValue<string>().Should().Contain("Name");

        var worksUi = GetUi(properties, "bekannteWerke");
        worksUi["variant"]!.GetValue<string>().Should().Be("pill-list");
        worksUi["tooltip"]!.GetValue<string>().Should().Contain("Werke");

        var promptUi = GetUi(properties, "bildPrompt");
        promptUi["variant"]!.GetValue<string>().Should().Be("muted");
        promptUi["order"]!.GetValue<int>().Should().Be(90);
        promptUi["tooltip"]!.GetValue<string>().Should().Contain("Prompt");
    }

    private static JsonObject GetUi(JsonObject properties, string key)
    {
        return properties[key]!["x-ui"]!.AsObject();
    }
}
