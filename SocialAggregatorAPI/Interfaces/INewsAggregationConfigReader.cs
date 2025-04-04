using SocialAggregatorAPI.Models;

public interface INewsAggregationConfigReader
{
    NewsAggregationSettings GetSettings();
}