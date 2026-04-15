using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FoodDelivery.Application.Mappings;

namespace FoodDelivery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(config => {
            config.AddMaps(typeof(MappingProfile).Assembly);
        });
        return services;
    }
}
