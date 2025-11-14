using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;
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
builder.Services.AddSingleton<CountrySchemaProvider>();
builder.Services.AddSingleton<INebiusService, NebiusService>();
builder.Services.AddSingleton<INebiusImageService, NebiusImageService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapGet("/schema", (CountrySchemaProvider provider) =>
{
    return Results.Json(provider.GetSchema());
});

api.MapPost("/analyze", async (
        CountryAnalysisRequest request,
        INebiusService nebiusService,
        INebiusImageService imageService,
        CancellationToken cancellationToken) =>
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Country))
        {
            return Results.BadRequest(new { message = "Bitte gib den Namen eines Landes an." });
        }

        try
        {
            var data = await nebiusService.GetCountryInformationAsync(request.Country, cancellationToken);
            ImageGenerationResult? image = null;
            string? imageError = null;

            var prompt = ExtractImagePrompt(data);
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                try
                {
                    image = await imageService.GenerateImageAsync(prompt, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    imageError = ex.Message;
                }
            }

            var response = new CountryAnalysisResponse
            {
                Country = request.Country,
                Data = data,
                Image = image,
                ImageError = imageError
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

app.MapFallbackToFile("index.html");

app.Run();

static string? ExtractImagePrompt(JsonObject data)
{
    if (data is null)
    {
        return null;
    }

    if (!data.TryGetPropertyValue("bildPrompt", out var promptNode))
    {
        return null;
    }

    if (promptNode is not JsonValue jsonValue)
    {
        return null;
    }

    if (!jsonValue.TryGetValue<string>(out var prompt))
    {
        return null;
    }

    var trimmed = prompt?.Trim();
    return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
}
