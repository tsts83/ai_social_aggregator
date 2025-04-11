namespace SocialAggregatorAPI.Models
{
    public class AppSettings
    {
        public NewsAggregationConfig? NewsAggregation { get; set; }
        public string? AiApiUrl { get; set; }
        public string? AiPrompt { get; set; }
        public string? AiMode { get; set; }
        public string? SocialMiniAppBaseUrl { get; set; }
        public string? SocialMiniAppUser { get; set; }
    }

    public class NewsAggregationConfig
    {
        public int FetchIntervalMinutes { get; set; }
        public int MaxArticlesPerFetch { get; set; }
        public FiltersConfig? Filters { get; set; }
        public SourcePreferencesConfig? SourcePreferences { get; set; }
        public PostSettingsConfig? PostSettings { get; set; }
    }

    public class FiltersConfig
    {
        public string? Language { get; set; }
        public List<string>? Keywords { get; set; }
    }

    public class SourcePreferencesConfig
    {
        public bool Reddit { get; set; }
        public bool NewsDataAPI { get; set; }
        public bool HackerNews { get; set; }
        public bool Nitter { get; set; }
    }

    public class PostSettingsConfig
    {
        public int PostIntervalMinutes { get; set; }
        public int MaxPostsPerDay { get; set; }
        public bool PosterServiceOn { get; set; }
    }
}
