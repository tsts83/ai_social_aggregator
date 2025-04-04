using System.Text.Json.Serialization;

public class NewsDataResponse
{
    public string? Status { get; set; }
    public int TotalResults { get; set; }
    public List<NewsDataArticle>? Results { get; set; }
}

public class NewsDataArticle
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("pubDate")]
    public string? PubDate { get; set; }
}