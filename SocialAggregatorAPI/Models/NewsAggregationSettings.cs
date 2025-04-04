namespace SocialAggregatorAPI.Models
{
    public class NewsAggregationSettings
    {
        public int FetchIntervalMinutes { get; set; } = 60;
        public int MaxArticlesPerFetch { get; set; } = 10;
        public FilterSettings Filters { get; set; } = new();
        public SourcePreferences SourcePreferences { get; set; } = new();
        public PostSettings PostSettings { get; set; } = new();
    }

    public class FilterSettings
    {
        public List<string> Keywords { get; set; } = new();
        public string Language { get; set; } = "en";
    }

    public class SourcePreferences
    {
        public bool Reddit { get; set; }
        public bool NewsDataAPI { get; set; }
        public bool HackerNews { get; set; }
        public bool Nitter { get; set; }
    }

    public class PostSettings
    {
        public int PostIntervalMinutes { get; set; } = 60;
        public int MaxPostsPerDay { get; set; } = 5;
    }
}
