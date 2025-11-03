using ContractAnalysisBlueprintPoC.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Images;
using System.ClientModel;

namespace ContractAnalysisBlueprintPoC.Services;

public sealed class NebiusImageService : INebiusImageService
{
    private readonly ILogger<NebiusImageService> _logger;
    private readonly NebiusOptions _options;
    private readonly ImageClient _client;

    public NebiusImageService(
        ILogger<NebiusImageService> logger,
        IOptions<NebiusOptions> optionsAccessor)
    {
        _logger = logger;
        _options = optionsAccessor.Value;

        var model = string.IsNullOrWhiteSpace(_options.ImageModel)
            ? _options.Model
            : _options.ImageModel;

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = _options.Endpoint,
            NetworkTimeout = TimeSpan.FromMinutes(4)
        };

        _client = new ImageClient(
            model: model,
            credential: new ApiKeyCredential(_options.ApiKey),
            options: clientOptions);

        _logger.LogDebug("Endpoint: {Endpoint}", _options.Endpoint);
        _logger.LogDebug("Image Model: {Model}", model);
    }

    public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Der Prompt darf nicht leer sein.", nameof(prompt));
        }

        var generationOptions = new ImageGenerationOptions
        {
            ResponseFormat = GeneratedImageFormat.Bytes,
            Size = new GeneratedImageSize(1024, 1024),
            Quality = new GeneratedImageQuality("high"),
            Style = GeneratedImageStyle.Vivid
        };

        ClientResult<GeneratedImage> result;
        try
        {
            result = await _client.GenerateImageAsync(prompt, generationOptions, cancellationToken);
        }
        catch (ClientResultException ex)
        {
            var response = ex.GetRawResponse();
            var body = response?.Content?.ToString() ?? "(no body)";
            _logger.LogError(ex, "Nebius image generation failed: {Status} {Body}", response?.Status, body);
            throw new InvalidOperationException("Nebius Bildgenerierung fehlgeschlagen.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Kommunikation mit der Nebius Bild-API.");
            throw;
        }

        var generatedImage = result.Value;
        if (generatedImage is null)
        {
            throw new InvalidOperationException("Die Nebius-Antwort enthielt kein Bild.");
        }

        var imageUrl = generatedImage.ImageUri?.ToString();
        var base64 = generatedImage.ImageBytes?.ToString();

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
