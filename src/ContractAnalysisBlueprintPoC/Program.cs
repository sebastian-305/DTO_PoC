using System.Text.Json;
using System.Text.Json.Serialization;
using ContractAnalysisBlueprintPoC.Blueprints;
using ContractAnalysisBlueprintPoC.Schema;
using ContractAnalysisBlueprintPoC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AnalysisBlueprintRegistry>();
builder.Services.AddSingleton<SampleResultProvider>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapGet("/blueprints", (AnalysisBlueprintRegistry registry) =>
    registry.GetAll().Select(bp => new
    {
        bp.Id,
        bp.DisplayName,
        SummaryFieldCount = bp.SummaryFields.Count,
        SectionCount = bp.Sections.Count
    }));

api.MapGet("/blueprints/{id}", (string id, AnalysisBlueprintRegistry registry) =>
    registry.GetById(id));

api.MapGet("/blueprints/{id}/schema", (string id, AnalysisBlueprintRegistry registry) =>
{
    var blueprint = registry.GetById(id);
    return SchemaBuilder.BuildResultSchema(blueprint);
});

api.MapGet("/samples/{id}", (string id, SampleResultProvider provider) =>
    provider.GetSample(id));

app.MapFallbackToFile("index.html");

app.Run();
