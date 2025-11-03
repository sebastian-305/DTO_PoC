using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContractAnalysisBlueprintPoC.Models;
using ContractAnalysisBlueprintPoC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<NebiusOptions>(builder.Configuration.GetSection("Nebius"));
builder.Services.AddSingleton<PersonSchemaProvider>();
builder.Services.AddSingleton<INebiusService, NebiusService>();
builder.Services.AddSingleton<INebiusImageService, NebiusImageService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapGet("/schema", (PersonSchemaProvider provider) =>
{
    return Results.Json(provider.GetSchema());
});

api.MapPost("/analyze", async (
        PersonAnalysisRequest request,
        INebiusService nebiusService,
        CancellationToken cancellationToken) =>
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Person))
        {
            return Results.BadRequest(new { message = "Bitte gib den Namen einer berühmten Person an." });
        }

        try
        {
            var data = await nebiusService.GetPersonInformationAsync(request.Person, cancellationToken);
            var response = new PersonAnalysisResponse
            {
                Person = request.Person,
                Data = data
            };

            return Results.Ok(response);
        }
        catch (ClientResultException ex)
        {
            var rawResponse = ex.GetRawResponse();
            var status = rawResponse?.Status ?? 502;
            var upstreamBody = rawResponse?.Content?.ToString();
            var detail = string.IsNullOrWhiteSpace(upstreamBody)
                ? ex.Message
                : string.Concat(ex.Message, Environment.NewLine, upstreamBody);

            return Results.Problem(
                title: "Nebius API Fehler",
                detail: detail,
                statusCode: status);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Analyse fehlgeschlagen",
                detail: ex.Message,
                statusCode: 500);
        }
    });

api.MapPost("/generate-image", async (
        ImageGenerationRequest request,
        INebiusImageService imageService,
        CancellationToken cancellationToken) =>
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            return Results.BadRequest(new { message = "Bitte gib einen Bild-Prompt an." });
        }

        try
        {
            var result = await imageService.GenerateImageAsync(request.Prompt, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                title: "Bildgenerierung fehlgeschlagen",
                detail: ex.Message,
                statusCode: 502);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Bildgenerierung fehlgeschlagen",
                detail: ex.Message,
                statusCode: 500);
        }
    });

app.MapFallbackToFile("index.html");

app.Run();
