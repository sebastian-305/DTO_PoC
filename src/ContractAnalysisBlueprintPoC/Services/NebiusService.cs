using System.ClientModel;
using System.Text;
using System.Text.Json;
using ContractAnalysisBlueprintPoC.Blueprints;
using ContractAnalysisBlueprintPoC.Schema;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace ContractAnalysisBlueprintPoC.Services;

public class NebiusService
{
    private const string PlaceholderModel = "placeholder-model-id";
    private const string PlaceholderApiKey = "placeholder-api-key";
    private static readonly Uri Endpoint = new("https://api.studio.nebius.com/v1/");
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(4);

    private readonly ILogger<NebiusService> _logger;
    private readonly ChatClient _client;

    public NebiusService(ILogger<NebiusService> logger)
    {
        _logger = logger;
        _client = new ChatClient(
            model: PlaceholderModel,
            credential: new ApiKeyCredential(PlaceholderApiKey),
            options: new OpenAIClientOptions
            {
                Endpoint = Endpoint,
                NetworkTimeout = TimeSpan.FromMinutes(10)
            });
    }

    public async Task<AnalysisResult> AnalyzeAsync(
        AnalysisBlueprint blueprint,
        string contractText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(contractText))
        {
            throw new ArgumentException("Der Vertragstext darf nicht leer sein.", nameof(contractText));
        }

        var schema = SchemaBuilder.BuildResultSchema(blueprint);
        var schemaData = BinaryData.FromString(schema.ToJsonString());

        var messages = BuildMessages(blueprint, contractText);
        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "Vertragsanalyse",
                jsonSchema: schemaData,
                jsonSchemaIsStrict: true),
            Temperature = 0f,
            TopP = 0.1f,
            MaxOutputTokenCount = 1200
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

        return ParseResult(content, blueprint);
    }

    private static IReadOnlyList<ChatMessage> BuildMessages(AnalysisBlueprint blueprint, string contractText)
    {
        var systemPrompt =
            "Du bist ein erfahrener österreichischer Rechtsanwalt. " +
            "Analysiere Verträge strikt nach Schema und gib ausschließlich gültiges JSON zurück.";

        var userPrompt = new StringBuilder()
            .AppendLine($"Blueprint: {blueprint.DisplayName}")
            .AppendLine("Kontext:")
            .AppendLine("- Prüfe den Vertrag auf unzulässige, nachteilige, widersprüchliche und vorteilhafte Klauseln.")
            .AppendLine("- Ergänze die Zusammenfassung mit den wichtigsten Eckdaten.")
            .AppendLine()
            .AppendLine("Vertragstext:")
            .AppendLine(contractText)
            .ToString();

        return new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userPrompt)
        };
    }

    private AnalysisResult ParseResult(string rawContent, AnalysisBlueprint blueprint)
    {
        var jsonText = ExtractJsonPayload(rawContent);
        using var document = JsonDocument.Parse(jsonText);
        var root = document.RootElement;

        var summary = BuildSummary(blueprint, root);
        var sections = BuildSections(blueprint, root);
        var conclusion = root.TryGetProperty("conclusion", out var conclusionElement)
            ? conclusionElement.GetString() ?? string.Empty
            : string.Empty;

        string? collectiveAgreement = null;
        if (root.TryGetProperty("collectiveAgreement", out var collectiveAgreementElement) &&
            collectiveAgreementElement.ValueKind != JsonValueKind.Null)
        {
            collectiveAgreement = collectiveAgreementElement.GetString();
        }

        return new AnalysisResult
        {
            BlueprintId = blueprint.Id,
            Summary = summary,
            Sections = EnsureAllSectionKeys(blueprint, sections),
            Conclusion = conclusion,
            CollectiveAgreement = collectiveAgreement
        };
    }

    private static Dictionary<string, string> BuildSummary(AnalysisBlueprint blueprint, JsonElement root)
    {
        var summary = new Dictionary<string, string>();
        var hasSummary = root.TryGetProperty("summary", out var summaryElement) &&
                         summaryElement.ValueKind == JsonValueKind.Object;

        foreach (var field in blueprint.SummaryFields)
        {
            if (hasSummary && summaryElement.TryGetProperty(field.Id, out var value))
            {
                summary[field.Id] = value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString() ?? string.Empty,
                    JsonValueKind.Number => value.ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => value.ToString()
                };
            }
            else
            {
                summary[field.Id] = string.Empty;
            }
        }

        return summary;
    }

    private static Dictionary<string, List<Dictionary<string, object?>>> BuildSections(
        AnalysisBlueprint blueprint,
        JsonElement root)
    {
        var result = new Dictionary<string, List<Dictionary<string, object?>>>();

        if (!root.TryGetProperty("sections", out var sectionsElement) ||
            sectionsElement.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in sectionsElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var entries = new List<Dictionary<string, object?>>();
            foreach (var item in property.Value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var entry = new Dictionary<string, object?>();
                foreach (var field in item.EnumerateObject())
                {
                    entry[field.Name] = ConvertJsonValue(field.Value);
                }

                entries.Add(entry);
            }

            result[property.Name] = entries;
        }

        return result;
    }

    private static object? ConvertJsonValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var longValue)
            ? longValue
            : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Array => element.EnumerateArray()
            .Select(value => value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : value.ToString())
            .ToList(),
        JsonValueKind.Null => null,
        _ => element.ToString()
    };

    private static Dictionary<string, List<Dictionary<string, object?>>> EnsureAllSectionKeys(
        AnalysisBlueprint blueprint,
        Dictionary<string, List<Dictionary<string, object?>>> sections)
    {
        foreach (var section in blueprint.Sections)
        {
            if (!sections.ContainsKey(section.Id))
            {
                sections[section.Id] = new List<Dictionary<string, object?>>();
            }
        }

        return sections;
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
