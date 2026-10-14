using System.Net;
using System.Text;
using System.Text.Json;
using Hirfolyam.Application.Features;
using Hirfolyam.Infrastructure.Analysis;
using Hirfolyam.Infrastructure.Options;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Hirfolyam.Tests.Infrastructure;

public sealed class OpenRouterHotTopicGeneratorTests
{
    private const string ValidTopics = """
        {"topics":[{"title":"Orosz dízel","summary":"Egy mondat.","articleIds":[1,2]}]}
        """;

    private static readonly HotTopicArticle[] Articles =
    [
        new(1, 1, "Telex", "Első cím", "Első bevezető"),
        new(2, 2, "Index", "Második cím", "Második bevezető")
    ];

    [Test]
    public async Task ValidResponseReturnsTheTopicsAndTheUsage()
    {
        var handler = new StubHandler(Envelope(ValidTopics));

        var result = await GenerateAsync(handler);
        var topic = result.Topics.Single();

        Assert.That(topic.Title, Is.EqualTo("Orosz dízel"));
        Assert.That(topic.Summary, Is.EqualTo("Egy mondat."));
        Assert.That(topic.ArticleIds, Is.EqualTo(new long[] { 1, 2 }));
        Assert.That(result.InputTokens, Is.EqualTo(10000));
        Assert.That(result.OutputTokens, Is.EqualTo(700));
        Assert.That(result.CostUsd, Is.EqualTo(0.0016m));
    }

    [Test]
    public async Task RequestContainsTheModelTheReasoningAndTheArticles()
    {
        var handler = new StubHandler(Envelope(ValidTopics));

        await GenerateAsync(handler, topicCount: 3);
        using var request = JsonDocument.Parse(handler.RequestBody!);
        var root = request.RootElement;
        using var input = JsonDocument.Parse(root.GetProperty("messages")[1].GetProperty("content").GetString()!);
        var first = input.RootElement[0];

        Assert.That(root.GetProperty("model").GetString(), Is.EqualTo("openai/gpt-6-luna"));
        Assert.That(root.GetProperty("max_tokens").GetInt32(), Is.EqualTo(16000));
        Assert.That(root.GetProperty("reasoning").GetProperty("effort").GetString(), Is.EqualTo("low"));
        Assert.That(root.TryGetProperty("temperature", out _), Is.False);
        Assert.That(root.GetProperty("messages")[0].GetProperty("content").GetString(), Does.Contain("Find the 3 hottest topics"));
        Assert.That(first.GetProperty("id").GetInt64(), Is.EqualTo(1));
        Assert.That(first.GetProperty("source").GetString(), Is.EqualTo("Telex"));
        Assert.That(first.GetProperty("title").GetString(), Is.EqualTo("Első cím"));
        Assert.That(first.GetProperty("lead").GetString(), Is.EqualTo("Első bevezető"));
    }

    [Test]
    public async Task EmptyReasoningEffortLeavesTheParameterOut()
    {
        var handler = new StubHandler(Envelope(ValidTopics));

        await GenerateAsync(handler, reasoningEffort: "");
        using var request = JsonDocument.Parse(handler.RequestBody!);

        Assert.That(request.RootElement.TryGetProperty("reasoning", out _), Is.False);
    }

    [Test]
    public async Task LongLeadsAreShortenedBeforeTheRequest()
    {
        var handler = new StubHandler(Envelope(ValidTopics));

        await GenerateAsync(handler, articles: [new HotTopicArticle(1, 1, "Telex", "Cím", new string('b', 5000))]);
        using var request = JsonDocument.Parse(handler.RequestBody!);
        using var input = JsonDocument.Parse(request.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!);

        Assert.That(input.RootElement[0].GetProperty("lead").GetString(), Has.Length.EqualTo(600));
    }

    [TestCase("{}")]
    [TestCase("null")]
    [TestCase("not JSON")]
    [TestCase("{\"topics\":null}")]
    [TestCase("{\"topics\":[{\"title\":\"T\",\"summary\":\"S\"}]}")]
    [TestCase("{\"topics\":[{\"title\":\"T\",\"summary\":\"S\",\"articleIds\":[\"1\"]}]}")]
    [TestCase("{\"topics\":[{\"title\":\"T\",\"summary\":\"S\",\"articleIds\":[1],\"extra\":1}]}")]
    public void InvalidTopicsAreRejected(string topics)
    {
        var handler = new StubHandler(Envelope(topics));

        Assert.That(() => GenerateAsync(handler), Throws.InstanceOf<Exception>());
    }

    [Test]
    public void IncompleteResponseIsRejectedEvenWithValidTopics()
    {
        var handler = new StubHandler(Envelope(ValidTopics, "length"));

        Assert.That(() => GenerateAsync(handler), Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public void ErrorResponseIsRejected()
    {
        var handler = new StubHandler("""{"error":{"message":"Rate limited"},"choices":[]}""");

        Assert.That(() => GenerateAsync(handler), Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public async Task DailyRequestContainsTheTopicsAndReturnsTheStories()
    {
        var handler = new StubHandler(Envelope("""
            {"topics":[{"title":"Napi történet","summary":"Két mondat. Itt a második.","topicIds":[11,12]}]}
            """));

        var result = await GenerateDailyAsync(handler, topicCount: 4);
        using var request = JsonDocument.Parse(handler.RequestBody!);
        var root = request.RootElement;
        var format = root.GetProperty("response_format").GetProperty("json_schema");
        using var input = JsonDocument.Parse(root.GetProperty("messages")[1].GetProperty("content").GetString()!);
        var story = result.Topics.Single();

        Assert.That(root.GetProperty("model").GetString(), Is.EqualTo("openai/gpt-6-luna"));
        Assert.That(root.GetProperty("messages")[0].GetProperty("content").GetString(), Does.Contain("Find the 4 most important stories of the day"));
        Assert.That(format.GetProperty("name").GetString(), Is.EqualTo("daily_topics"));
        Assert.That(format.GetProperty("schema").GetRawText(), Does.Contain("\"topicIds\""));
        Assert.That(input.RootElement.GetArrayLength(), Is.EqualTo(2));
        Assert.That(input.RootElement[0].GetProperty("id").GetInt64(), Is.EqualTo(11));
        Assert.That(input.RootElement[0].GetProperty("title").GetString(), Is.EqualTo("Reggeli téma"));
        Assert.That(input.RootElement[0].GetProperty("summary").GetString(), Is.EqualTo("Reggeli mondat."));
        Assert.That(story.Title, Is.EqualTo("Napi történet"));
        Assert.That(story.Summary, Is.EqualTo("Két mondat. Itt a második."));
        Assert.That(story.TopicIds, Is.EqualTo(new long[] { 11, 12 }));
        Assert.That(result.InputTokens, Is.EqualTo(10000));
        Assert.That(result.CostUsd, Is.EqualTo(0.0016m));
    }

    [Test]
    public void DailyStoriesWithArticleIdsAreRejected()
    {
        var handler = new StubHandler(Envelope(ValidTopics));

        Assert.That(() => GenerateDailyAsync(handler), Throws.InstanceOf<Exception>());
    }

    [TestCase("")]
    [TestCase("  ")]
    public void MissingApiKeyFailsWithoutARequest(string apiKey)
    {
        var handler = new StubHandler(Envelope(ValidTopics));

        Assert.That(() => GenerateAsync(handler, apiKey: apiKey), Throws.TypeOf<InvalidOperationException>());
        Assert.That(handler.RequestBody, Is.Null);
    }

    private static async Task<HotTopicGeneration> GenerateAsync(
        StubHandler handler,
        int topicCount = 5,
        string reasoningEffort = "low",
        HotTopicArticle[]? articles = null,
        string apiKey = "test-key")
    {
        using var client = new HttpClient(handler);
        var generator = new OpenRouterHotTopicGenerator(
            client,
            OptionsFactory.Create(new OpenRouterOptions
            {
                ApiKey = apiKey,
                HotTopics = new HotTopicModelOptions { ReasoningEffort = reasoningEffort }
            }));

        return await generator.GenerateAsync(articles ?? Articles, topicCount, CancellationToken.None);
    }

    private static async Task<DailyTopicGeneration> GenerateDailyAsync(StubHandler handler, int topicCount = 5)
    {
        using var client = new HttpClient(handler);
        var generator = new OpenRouterHotTopicGenerator(
            client,
            OptionsFactory.Create(new OpenRouterOptions { ApiKey = "test-key" }));

        return await generator.GenerateDailyAsync(
            [
                new DailyTopicInput(11, "Reggeli téma", "Reggeli mondat."),
                new DailyTopicInput(12, "Délutáni téma", "Délutáni mondat.")
            ],
            topicCount,
            CancellationToken.None);
    }

    private static string Envelope(string topics, string finishReason = "stop")
    {
        return JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    finish_reason = finishReason,
                    message = new { content = topics }
                }
            },
            usage = new
            {
                prompt_tokens = 10000,
                completion_tokens = 700,
                cost = 0.0016m
            }
        });
    }

    private sealed class StubHandler(string body) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }
    }
}
