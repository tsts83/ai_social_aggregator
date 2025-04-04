using Microsoft.Extensions.Options;
using SocialAggregatorAPI.Models;

namespace SocialAggregatorAPI
{
    public class NewsAggregationConfigReader : INewsAggregationConfigReader
    {
        private readonly NewsAggregationSettings _settings;

        public NewsAggregationConfigReader(IOptions<NewsAggregationSettings> options)
        {
            _settings = options.Value;
        }

        public NewsAggregationSettings GetSettings()
        {
            return _settings;
        }
    }
}
