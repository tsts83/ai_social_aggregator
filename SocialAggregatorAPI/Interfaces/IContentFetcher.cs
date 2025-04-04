using SocialAggregatorAPI.Data;

public interface IContentFetcher
{    
    Task<AppDbContext> FetchNewsDataApiNews(AppDbContext dbContext, HttpClient httpClient);

}