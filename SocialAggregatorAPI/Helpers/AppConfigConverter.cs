using AutoMapper;
using SocialAggregatorAPI.Models;

public class AppConfigConverter : ITypeConverter<IEnumerable<AppConfig>, AppSettings>
{
    public AppSettings Convert(IEnumerable<AppConfig> source, AppSettings destination, ResolutionContext context)
    {
        var config = new AppSettings();

        var dict = source.ToDictionary(c => $"{c.Section}.{c.Key}", c => c.Value)
                         .Where(kv => kv.Value != null)
                         .ToDictionary(kv => kv.Key, kv => kv.Value!);

        // Flat configs
        config.AiApiUrl = dict.GetValueOrDefault("AI.ApiUrl");
        config.AiPrompt = dict.GetValueOrDefault("AI.Prompt");
        config.AiMode = dict.GetValueOrDefault("AI.Mode");
        config.SocialMiniAppBaseUrl = dict.GetValueOrDefault("SocialMiniApp.BaseUrl");
        config.SocialMiniAppUser = dict.GetValueOrDefault("SocialMiniApp.User");

        // NewsAggregation
        config.NewsAggregation = new NewsAggregationConfig
        {
            FetchIntervalMinutes = dict.ParseInt("NewsAggregation.FetchIntervalMinutes"),
            MaxArticlesPerFetch = dict.ParseInt("NewsAggregation.MaxArticlesPerFetch"),
            Filters = new FiltersConfig
            {
                Language = dict.GetValueOrDefault("NewsAggregation.Filters.Language"),
                Keywords = dict.ParseList("NewsAggregation.Filters.Keywords")
            },
            SourcePreferences = new SourcePreferencesConfig
            {
                Reddit = dict.ParseBool("NewsAggregation.SourcePreferences.Reddit"),
                NewsDataAPI = dict.ParseBool("NewsAggregation.SourcePreferences.NewsDataAPI"),
                HackerNews = dict.ParseBool("NewsAggregation.SourcePreferences.HackerNews"),
                Nitter = dict.ParseBool("NewsAggregation.SourcePreferences.Nitter")
            },
            PostSettings = new PostSettingsConfig
            {
                PostIntervalMinutes = dict.ParseInt("NewsAggregation.PostSettings.PostIntervalMinutes"),
                MaxPostsPerDay = dict.ParseInt("NewsAggregation.PostSettings.MaxPostsPerDay"),
                PosterServiceOn = dict.ParseBool("NewsAggregation.PostSettings.PosterServiceOn")
            }
        };

        return config;
    }
}
