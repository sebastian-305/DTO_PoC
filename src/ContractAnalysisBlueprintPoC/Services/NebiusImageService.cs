using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using ContractAnalysisBlueprintPoC.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContractAnalysisBlueprintPoC.Services;

public sealed class NebiusImageService : INebiusImageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<NebiusImageService> _logger;
    private readonly NebiusOptions _options;

    public NebiusImageService(
        HttpClient httpClient,
        ILogger<NebiusImageService> logger,
        IOptions<NebiusOptions> optionsAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = optionsAccessor.Value;

        _httpClient.Timeout = TimeSpan.FromMinutes(4);
        _httpClient.BaseAddress = _options.Endpoint;
        if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
        _logger.LogDebug("API Key: {key}", _options.ApiKey);
    }

    public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Der Prompt darf nicht leer sein.", nameof(prompt));
        }

        var requestBody = new JsonObject
        {
            ["model"] = string.IsNullOrWhiteSpace(_options.ImageModel) ? _options.Model : _options.ImageModel,
            ["prompt"] = prompt,
            ["size"] = "1024x1024",
            ["style"] = "vivid",
            ["quality"] = "high",
            ["response_format"] = "url"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "images")
        {
            Content = JsonContent.Create(requestBody, options: SerializerOptions),
            RequestUri = new Uri("https://api.studio.nebius.com/v1/")
        
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Nebius image generation failed: {Status} {Body}",
                (int)response.StatusCode,
                content);
            throw new InvalidOperationException($"Nebius Bildgenerierung fehlgeschlagen (Status {(int)response.StatusCode}).");
        }

        JsonObject payload;
        try
        {
            payload = JsonNode.Parse(content)?.AsObject()
                ?? throw new InvalidOperationException("Die Antwort konnte nicht als JSON gelesen werden.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Nebius image generation returned invalid JSON: {Body}", content);
            throw new InvalidOperationException("Die Antwort der Bildgenerierung war ungültig.", ex);
        }

        var imageNode = payload["data"]?.AsArray().FirstOrDefault() as JsonObject;
        if (imageNode is null)
        {
            throw new InvalidOperationException("Die Nebius-Antwort enthielt kein Bild.");
        }

        var imageUrl = imageNode["url"]?.GetValue<string>();
        var base64 = imageNode["b64_json"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(imageUrl) && string.IsNullOrWhiteSpace(base64))
        {
            throw new InvalidOperationException("Die Nebius-Antwort enthält weder eine Bild-URL noch Base64-Daten.");
        }

        return new ImageGenerationResult
        {
            Prompt = prompt,
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
            ImageBase64 = string.IsNullOrWhiteSpace(base64) ? null : base64,
            MediaType = "image/png"
        };
    }
}
