using SocialAggregatorAPI.Models;

public interface IAppConfigService
{
    Task<AppSettings> GetConfigAsync();
    Task RefreshConfigAsync();
    AppSettings CurrentConfig { get; }
}