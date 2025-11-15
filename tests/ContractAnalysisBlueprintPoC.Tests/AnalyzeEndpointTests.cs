using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContractAnalysisBlueprintPoC.Models;
using ContractAnalysisBlueprintPoC.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContractAnalysisBlueprintPoC.Tests;

public class AnalyzeEndpointTests
{
    [Fact]
    public async Task Analyze_WhenNebiusReturnsClientError_PropagatesStatus()
    {
        const int expectedStatus = 400;
        const string upstreamBody = "{\"error\":\"invalid\"}";

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<INebiusService>(new ThrowingNebiusService(expectedStatus, upstreamBody));
                services.AddSingleton<INebiusImageService>(new FakeImageService());
            });
        });

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/analyze",
            new { type = "country", country = "Testland" });

        response.StatusCode.Should().Be((HttpStatusCode)expectedStatus);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(expectedStatus);
        problem.Detail.Should().Contain(upstreamBody);
    }

    [Fact]
    public async Task Analyze_WhenPromptMissesCountryName_AppendsQueryBeforeImageGeneration()
    {
        const string country = "Testland";
        var recorder = new RecordingImageService();

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<INebiusService>(new FixedCountryNebiusService());
                services.AddSingleton<INebiusImageService>(recorder);
            });
        });

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/analyze",
            new { type = "country", country });

        response.EnsureSuccessStatusCode();
        recorder.LastPrompt.Should().NotBeNull();
        recorder.LastPrompt.Should().Contain(country);
    }

    private sealed class FakeImageService : INebiusImageService
    {
        public Task<ImageGenerationResult> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
            => Task.FromResult(new ImageGenerationResult
            {
                Prompt = prompt,
                ImageUrl = "https://example.test/image.png",
                MediaType = "image/png"
            });
    }

    private sealed class RecordingImageService : INebiusImageService
    {
        public string? LastPrompt { get; private set; }

        public Task<ImageGenerationResult> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
        {
            LastPrompt = prompt;
            return Task.FromResult(new ImageGenerationResult
            {
                Prompt = prompt,
                ImageUrl = "https://example.test/recorded.png",
                MediaType = "image/png"
            });
        }
    }

    private sealed class ThrowingNebiusService(int status, string body) : INebiusService
    {
        public Task<JsonObject> GetCountryInformationAsync(string country, CancellationToken cancellationToken = default)
        {
            throw new ClientResultException(
                message: "Nebius request failed.",
                response: new FakePipelineResponse(status, body));
        }

        public Task<JsonObject> GetPersonInformationAsync(string person, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FixedCountryNebiusService : INebiusService
    {
        public Task<JsonObject> GetCountryInformationAsync(string country, CancellationToken cancellationToken = default)
        {
            var data = new JsonObject
            {
                ["bildPrompt"] = "Pulsierende Altstadt mit engen Gassen und bunten Märkten"
            };

            return Task.FromResult(data);
        }

        public Task<JsonObject> GetPersonInformationAsync(string person, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FakePipelineResponse : PipelineResponse
    {
        private readonly PipelineResponseHeaders _headers = new FakeHeaders();
        private readonly BinaryData _content;
        private readonly int _status;

        public FakePipelineResponse(int status, string body)
        {
            _status = status;
            _content = BinaryData.FromString(body);
        }

        public override int Status => _status;

        public override string ReasonPhrase => string.Empty;

        protected override PipelineResponseHeaders HeadersCore => _headers;

        public override Stream? ContentStream { get; set; }

        public override BinaryData Content => _content;

        public override BinaryData BufferContent(CancellationToken cancellationToken = default) => _content;

        public override ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult(_content);

        public override void Dispose()
        {
        }
    }

    private sealed class FakeHeaders : PipelineResponseHeaders
    {
        public override bool TryGetValue(string name, out string? value)
        {
            value = null;
            return false;
        }

        public override bool TryGetValues(string name, out IEnumerable<string>? values)
        {
            values = null;
            return false;
        }

        public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();
    }
}
