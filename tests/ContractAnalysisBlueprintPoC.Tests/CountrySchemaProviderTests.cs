using System.Linq;
using System.Text.Json.Nodes;
using ContractAnalysisBlueprintPoC.Services;
using FluentAssertions;
using Xunit;

namespace ContractAnalysisBlueprintPoC.Tests;

public class CountrySchemaProviderTests
{
    private readonly CountrySchemaProvider _provider = new();

    [Fact]
    public void GetSchema_ShouldReturnClone()
    {
        var first = _provider.GetSchema();
        var second = _provider.GetSchema();

        second["properties"]! ["hauptstadt"]! ["description"] = "verändert";

        first["properties"]! ["hauptstadt"]! ["description"]!
            .GetValue<string>()
            .Should().NotBe("verändert");
    }

    [Fact]
    public void Schema_ShouldContainRequiredFields()
    {
        var schema = _provider.GetSchema();

        schema["type"]!.GetValue<string>().Should().Be("object");
        schema["additionalProperties"]!.GetValue<bool>().Should().BeFalse();

        var required = schema["required"]!.AsArray().Select(node => node!.GetValue<string>()).ToArray();
        required.Should().BeEquivalentTo(new[]
        {
            "name",
            "hauptstadt",
            "einwohnerzahl",
            "flaeche",
            "amtssprachen",
            "kontinent",
            "kurzbeschreibung",
            "bildPrompt"
        });

        var properties = schema["properties"]!.AsObject();
        properties.Should().ContainKeys(required.Append("staatsform"));

        properties["amtssprachen"]!["type"]!.GetValue<string>().Should().Be("array");
        properties["amtssprachen"]!["items"]!.AsObject()["type"]!.GetValue<string>().Should().Be("string");

        properties["einwohnerzahl"]!["type"]!.GetValue<string>().Should().Be("string");
        properties["flaeche"]!["type"]!.GetValue<string>().Should().Be("string");

        var bildPrompt = properties["bildPrompt"]!.AsObject();
        bildPrompt["type"]!.GetValue<string>().Should().Be("string");
        bildPrompt["description"]!.GetValue<string>().Should().NotBeNullOrWhiteSpace();
    }
}
