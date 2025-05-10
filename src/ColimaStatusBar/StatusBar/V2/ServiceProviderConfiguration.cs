using Microsoft.Extensions.DependencyInjection;

namespace ColimaStatusBar.StatusBar.V2;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddStatusBar(this IServiceCollection services)
    {
        return services.AddScoped<MainDelegate>()
            .AddScoped<CurrentProfile>();
    }
}
