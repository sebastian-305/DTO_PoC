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
builder.Services.AddSingleton<CountrySchemaProvider>();
builder.Services.AddSingleton<INebiusService, NebiusService>();
builder.Services.AddSingleton<INebiusImageService, NebiusImageService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapGet("/schema", (AnalysisType? type, PersonSchemaProvider personProvider, CountrySchemaProvider countryProvider) =>
    {
        var target = type ?? AnalysisType.Person;
        var schema = target == AnalysisType.Country
            ? countryProvider.GetSchema()
            : personProvider.GetSchema();

        return Results.Json(schema);
    });

api.MapPost("/analyze", async (
        AnalysisRequest request,
        INebiusService nebiusService,
        CancellationToken cancellationToken) =>
    {
        if (request is null)
        {
            return Results.BadRequest(new { message = "Bitte gib einen Suchbegriff an." });
        }

        var type = request.Type;

        try
        {
            return type switch
            {
                AnalysisType.Person => await HandlePersonAsync(request, nebiusService, cancellationToken),
                AnalysisType.Country => await HandleCountryAsync(request, nebiusService, cancellationToken),
                _ => Results.BadRequest(new { message = "Der angegebene Analysetyp wird nicht unterstützt." })
            };
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

static async Task<IResult> HandlePersonAsync(AnalysisRequest request, INebiusService nebiusService, CancellationToken cancellationToken)
{
    var person = request.Person?.Trim();
    if (string.IsNullOrWhiteSpace(person))
    {
        return Results.BadRequest(new { message = "Bitte gib den Namen einer berühmten Person an." });
    }

    var data = await nebiusService.GetPersonInformationAsync(person, cancellationToken);
    var response = new AnalysisResponse
    {
        Type = AnalysisType.Person,
        Query = person,
        Data = data
    };

    return Results.Ok(response);
}

static async Task<IResult> HandleCountryAsync(AnalysisRequest request, INebiusService nebiusService, CancellationToken cancellationToken)
{
    var country = request.Country?.Trim();
    if (string.IsNullOrWhiteSpace(country))
    {
        return Results.BadRequest(new { message = "Bitte gib den Namen eines Landes an." });
    }

    var data = await nebiusService.GetCountryInformationAsync(country, cancellationToken);
    var response = new AnalysisResponse
    {
        Type = AnalysisType.Country,
        Query = country,
        Data = data
    };

    return Results.Ok(response);
}
