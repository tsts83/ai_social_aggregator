using AutoMapper;
using SocialAggregatorAPI.Models;

public class AppConfigService : IAppConfigService
{
    private readonly IAppConfigRepository _repository;
    private readonly ILogger<AppConfigService> _logger;
    private AppSettings? _cachedConfig;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private AppSettings _currentConfig;
    private readonly IMapper _mapper;


    public AppConfigService ( IMapper mapper, IAppConfigRepository repository, ILogger<AppConfigService> logger)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        // Initial load (you could also do this lazily if preferred)
        _currentConfig = LoadConfigAsync().GetAwaiter().GetResult();
    }

    public async Task<AppSettings> GetConfigAsync()
    {
        if (_cachedConfig == null)
        {
            await _lock.WaitAsync();
            try
            {
                if (_cachedConfig == null)
                {
                    _cachedConfig = await LoadConfigAsync();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        return _cachedConfig;
    }

    public async Task RefreshConfigAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _cachedConfig = await LoadConfigAsync();
            _logger.LogInformation("App configuration refreshed at {Time}", DateTime.UtcNow);
        }
        finally
        {
            _lock.Release();
        }
    }

    public AppSettings CurrentConfig => _currentConfig;

    private async Task<AppSettings> LoadConfigAsync()
    {
            var configs = await _repository.GetAllConfigsAsync();
            return _mapper.Map<AppSettings>(configs);
    }


    public async Task ReloadConfigAsync()
    {
        _currentConfig = await LoadConfigAsync();
    }
}