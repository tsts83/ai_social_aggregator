public interface IAppConfigRepository
{
    Task<IEnumerable<AppConfig>> GetAllConfigsAsync();
}