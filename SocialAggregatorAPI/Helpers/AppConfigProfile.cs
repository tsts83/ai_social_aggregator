using AutoMapper;
using SocialAggregatorAPI.Models;

public class AppConfigProfile : Profile
{
    public AppConfigProfile()
    {
        CreateMap<IEnumerable<AppConfig>, AppSettings>()
            .ConvertUsing<AppConfigConverter>();
    }
}