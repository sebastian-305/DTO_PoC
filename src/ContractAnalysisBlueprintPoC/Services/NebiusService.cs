using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace ContractAnalysisBlueprintPoC.Services;

public sealed class NebiusService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(4);

    private readonly ILogger<NebiusService> _logger;
    private readonly CountrySchemaProvider _schemaProvider;
    private readonly ChatClient _client;

    public NebiusService(
        ILogger<NebiusService> logger,
        CountrySchemaProvider schemaProvider,
        IOptions<NebiusOptions> optionsAccessor)
    {
        _logger = logger;
        _schemaProvider = schemaProvider;

        var options = optionsAccessor.Value;
        _client = new ChatClient(
            model: options.Model,
            credential: new ApiKeyCredential(options.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = options.Endpoint,
                NetworkTimeout = TimeSpan.FromMinutes(10)
            });
    }

    public async Task<JsonObject> GetCountryInformationAsync(string country, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Der Landesname darf nicht leer sein.", nameof(country));
        }

        var schema = _schemaProvider.GetSchema();
        var schemaData = BinaryData.FromString(schema.ToJsonString());

        var messages = new[]
        {
            ChatMessage.CreateSystemMessage("Du bist ein zuverlässiger Geograf und musst streng das geforderte JSON-Schema einhalten."),
            ChatMessage.CreateUserMessage($"Analysiere das Land '{country}'. Liefere ausschließlich Fakten im JSON-Format.")
        };

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "country_information",
                jsonSchema: schemaData,
                jsonSchemaIsStrict: true),
            Temperature = 0f,
            TopP = 0.1f,
            MaxOutputTokenCount = 800
        };

        ChatCompletion completion;
        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(RequestTimeout);
            completion = await _client.CompleteChatAsync(messages, options, linkedCts.Token).ConfigureAwait(false);
        }
        catch (ClientResultException ex)
        {
            var response = ex.GetRawResponse();
            var body = response?.Content?.ToString() ?? "(no body)";
            _logger.LogError(ex, "Nebius API error: {Status} {Body}", response?.Status, body);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Kommunikation mit der Nebius API.");
            throw;
        }

        _logger.LogInformation(
            "Nebius Token Usage - Input: {Input}, Output: {Output}, Total: {Total}",
            completion.Usage?.InputTokenCount,
            completion.Usage?.OutputTokenCount,
            completion.Usage?.TotalTokenCount);

        var content = completion.Content.Count > 0 ? completion.Content[0].Text : null;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Die Nebius-Antwort enthielt keinen Inhalt.");
        }

        return ParseJson(content);
    }

    private static JsonObject ParseJson(string rawContent)
    {
        var jsonText = ExtractJsonPayload(rawContent);
        var node = JsonNode.Parse(jsonText) as JsonObject;
        if (node is null)
        {
            throw new InvalidOperationException("Das Ergebnis konnte nicht als JSON-Objekt gelesen werden.");
        }

        return node;
    }

    private static string ExtractJsonPayload(string rawContent)
    {
        var trimmed = rawContent.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var newlineIndex = trimmed.IndexOf('\n');
        if (newlineIndex < 0)
        {
            return trimmed;
        }

        var withoutFence = trimmed[(newlineIndex + 1)..];
        var closingFenceIndex = withoutFence.LastIndexOf("```", StringComparison.Ordinal);
        if (closingFenceIndex >= 0)
        {
            withoutFence = withoutFence[..closingFenceIndex];
        }

        return withoutFence.Trim();
    }
}
