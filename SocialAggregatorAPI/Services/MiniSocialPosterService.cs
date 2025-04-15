using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;

public class MiniSocialPosterService : BackgroundService, IMiniSocialPosterService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MiniSocialPosterService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly AppSettings _settings;
    private readonly IAppConfigService _appConfigService;
    private string _message = string.Empty;
    private const string loginEndpoint = "/auth/login";
    private const string postEndpoint = "/posts";

    public MiniSocialPosterService(
        IHttpClientFactory httpClientFactory,
        ILogger<MiniSocialPosterService> logger,
        IAppConfigService appConfigService,
        IConfiguration config,
        IServiceScopeFactory scopeFactory)
    {
        _appConfigService = appConfigService;
        _settings = appConfigService.CurrentConfig;
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = await _appConfigService.GetConfigAsync();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PostNextUnpostedArticleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MiniSocialPosterService");
            }

            var postIntervalMinutes = settings.NewsAggregation?.PostSettings?.PostIntervalMinutes ?? 1; // Default to 1 minute if null
            await Task.Delay(TimeSpan.FromMinutes(postIntervalMinutes), stoppingToken);
        }
    }

    public async Task<string> PostNextUnpostedArticleAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _appConfigService.GetConfigAsync();
        // Check if the service is enabled
        if (!settings.NewsAggregation?.PostSettings?.PosterServiceOn ?? false)
        {
            _message =" Poster service is turned off, skipping posting.";
            _logger.LogInformation(_message);
            return _message;
        }

        var token = await AuthenticateAsync();

        if (string.IsNullOrEmpty(token))
        {
            _message = "Authentication failed. Skipping this cycle.";
            _logger.LogWarning(_message);
            return _message;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var article = await dbContext.NewsArticles
            .Where(a => !a.IsPosted)
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (article == null)
        {
            _message = "No unposted articles found.";
            _logger.LogInformation(_message);
            return _message;
        }

        var imageBytes = string.IsNullOrEmpty(article.ThumbnailUrl)
            ? null
            : await DownloadAndCompressImageAsync(article.ThumbnailUrl);

        var success = await PostToMiniSocialMediaAsync(article, token, imageBytes);

        if (success)
        {
            article.IsPosted = true;
            dbContext.Update(article);
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Article posted: {ArticleTitle}", article.Title);
            _message = $"Article posted: {article.Title}";
            return _message;
        }
        else
        {
            _message = $"Failed to post article: {article.Title}";
            _logger.LogWarning(_message);
            return _message;
        }
    }

    private async Task<string?> AuthenticateAsync()
    {
        var settings = await _appConfigService.GetConfigAsync();
        var client = _httpClientFactory.CreateClient();

        var baseUrl = _settings.SocialMiniAppBaseUrl ?? string.Empty;
        var response = await client.PostAsJsonAsync($"{baseUrl}{loginEndpoint}", new
        {
            email = settings.SocialMiniAppUser,
            password = _config["miniAppUserPassword"] ?? Environment.GetEnvironmentVariable("MINIAPP_USER_PASSWORD")
        });

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Authentication failed with status code: {response.StatusCode}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("token", out var tokenElement))
        {
            return tokenElement.GetString();
        }

        _logger.LogWarning("Token not found in authentication response.");
        return null;
    }

    private async Task<byte[]?> DownloadAndCompressImageAsync(string imageUrl)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var imageBytes = await client.GetByteArrayAsync(imageUrl);

            using var image = Image.Load(imageBytes);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(800, 600),
                Mode = ResizeMode.Max
            }));

            using var ms = new MemoryStream();
            var encoder = new JpegEncoder { Quality = 75 };
            await image.SaveAsJpegAsync(ms, encoder);

            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download or compress image from {ImageUrl}", imageUrl);
            return null;
        }
    }

    private async Task<bool> PostToMiniSocialMediaAsync(NewsArticle article, string token, byte[]? imageBytes)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent(article.AiSummary ?? string.Empty), "text");

            if (imageBytes != null)
            {
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                multipart.Add(imageContent, "image", "image.jpg");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var baseUrl = _settings.SocialMiniAppBaseUrl ?? string.Empty;
            var response = await client.PostAsync($"{baseUrl}{postEndpoint}", multipart);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post article {ArticleTitle}", article.Title);
            return false;
        }
    }
}