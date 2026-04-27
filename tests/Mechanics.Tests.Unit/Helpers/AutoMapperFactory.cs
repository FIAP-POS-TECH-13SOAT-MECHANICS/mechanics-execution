using AutoMapper;
using Mechanics.Application.Auth;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mechanics.Tests.Unit.Helpers;

public static class AutoMapperFactory
{
    public static IMapper CreateMap(string domain)
    {
        var config = new MapperConfiguration(cfg =>
        {
            var profiles = typeof(AuthMapperProfile).Assembly.GetTypes()
                .Where(type => (type.Namespace?.Contains(domain) ?? false) && type.BaseType == typeof(Profile));

            foreach (var profile in profiles)
                cfg.AddProfile(profile);
        }, new NullLoggerFactory());

        return config.CreateMapper();
    }
}
