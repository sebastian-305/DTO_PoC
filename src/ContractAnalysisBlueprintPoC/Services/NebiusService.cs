using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace ContractAnalysisBlueprintPoC.Services;

public sealed class NebiusService : INebiusService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(4);

    private readonly ILogger<NebiusService> _logger;
    private readonly PersonSchemaProvider _personSchemaProvider;
    private readonly CountrySchemaProvider _countrySchemaProvider;
    private readonly ChatClient _client;

    public NebiusService(
        ILogger<NebiusService> logger,
        PersonSchemaProvider personSchemaProvider,
        CountrySchemaProvider countrySchemaProvider,
        IOptions<NebiusOptions> optionsAccessor)
    {
        _logger = logger;
        _personSchemaProvider = personSchemaProvider;
        _countrySchemaProvider = countrySchemaProvider;

        var options = optionsAccessor.Value;
        _client = new ChatClient(
            model: options.Model,
            credential: new ApiKeyCredential(options.ApiKey),
            options: new OpenAIClientOptions()
            {
                Endpoint = options.Endpoint,
                NetworkTimeout = TimeSpan.FromMinutes(10)
            });
        _logger.LogDebug("Endpoint:");
        _logger.LogDebug(options.Endpoint.ToString());
        _logger.LogDebug(options.Model);
        _logger.LogDebug(options.ApiKey);
    }

    public async Task<JsonObject> GetPersonInformationAsync(string person, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(person))
        {
            throw new ArgumentException("Der Name der Person darf nicht leer sein.", nameof(person));
        }

        var schema = _personSchemaProvider.GetSchema();
        var systemPrompt =
            "Du bist ein gewissenhafter Biograf. Halte dich strikt an das geforderte JSON-Schema. " +
            "Der Schlüssel `bildPrompt` muss die Person mit Namen, äußeren Merkmalen, typischem Outfit und Stimmung beschreiben, " +
            "damit ein Bildgenerator die Person authentisch darstellen kann.";
        var userPrompt =
            $"Stelle die berühmte Person \"{person}\" vor und liefere ausschließlich Fakten im JSON-Format. " +
            "Der Eintrag `bildPrompt` soll die Person mit Namen und charakteristischen Merkmalen beschreiben.";

        return await RequestStructuredDataAsync(
            person,
            schema,
            schemaFormatLabel: "person_information",
            systemPrompt,
            userPrompt,
            cancellationToken);
    }

    public async Task<JsonObject> GetCountryInformationAsync(string country, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Der Name des Landes darf nicht leer sein.", nameof(country));
        }

        var schema = _countrySchemaProvider.GetSchema();
        var systemPrompt =
            "Du bist eine sachliche Landesexpertin. Halte dich strikt an das geforderte JSON-Schema. " +
            "Der Schlüssel `bildPrompt` beschreibt ein typisches Landschafts- oder Stadtmotiv, das das Land repräsentiert.";
        var userPrompt =
            $"Analysiere das Land \"{country}\" und liefere ausschließlich Fakten im JSON-Format. " +
            "Der Eintrag `bildPrompt` soll ein fotorealistisches Motiv mit Stimmung, Tageszeit und markanten Details beschreiben.";

        return await RequestStructuredDataAsync(
            country,
            schema,
            schemaFormatLabel: "country_information",
            systemPrompt,
            userPrompt,
            cancellationToken);
    }

    private async Task<JsonObject> RequestStructuredDataAsync(
        string input,
        JsonObject schema,
        string schemaFormatLabel,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Requesting structured data for {Schema} with input '{Input}'", schemaFormatLabel, input);
        var schemaData = BinaryData.FromString(schema.ToJsonString());
        var messages = new ChatMessage[]
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userPrompt)
        };

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: schemaFormatLabel,
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
            completion = await _client.CompleteChatAsync(messages, options, linkedCts.Token);
        }
        catch (ClientResultException ex)
        {
            var response = ex.GetRawResponse();
            _logger.LogError(ex.Message);
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

        _logger.LogDebug(content);
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
