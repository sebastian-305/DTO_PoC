using System.Text.Json;
using System.Text.Json.Serialization;
using System.ClientModel;
using ContractAnalysisBlueprintPoC.Blueprints;
using ContractAnalysisBlueprintPoC.Models;
using ContractAnalysisBlueprintPoC.Schema;
using ContractAnalysisBlueprintPoC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AnalysisBlueprintRegistry>();
builder.Services.AddSingleton<SampleResultProvider>();
builder.Services.AddSingleton<NebiusService>();
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

api.MapPost("/blueprints/{id}/analysis", async (
        string id,
        AnalysisRequest request,
        AnalysisBlueprintRegistry registry,
        NebiusService nebiusService,
        CancellationToken cancellationToken) =>
    {
        if (request is null || string.IsNullOrWhiteSpace(request.ContractText))
        {
            return Results.BadRequest(new { message = "Der Vertragstext darf nicht leer sein." });
        }

        var blueprint = registry.GetById(id);
        try
        {
            var result = await nebiusService.AnalyzeAsync(blueprint, request.ContractText, cancellationToken);
            return Results.Ok(result);
        }
        catch (ClientResultException ex)
        {
            return Results.Problem(
                title: "Nebius API Fehler",
                detail: ex.Message,
                statusCode: 502);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Analyse fehlgeschlagen",
                detail: ex.Message,
                statusCode: 500);
        }
    });

app.MapFallbackToFile("index.html");

app.Run();
