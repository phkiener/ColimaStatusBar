using Microsoft.Extensions.DependencyInjection;

namespace ColimaStatusBar.StatusBar;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddStatusBar(this IServiceCollection services)
    {
        return services.AddScoped<AppDelegate>()
            .AddScoped<CurrentProfileControl>()
            .AddScoped<RunningContainersControl>()
            .AddScoped<SettingsControl>();
    }
}
