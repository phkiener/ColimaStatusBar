using ColimaStatusBar.Ui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ColimaStatusBar.Ui;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddStatusBar(this IServiceCollection services)
    {
        return services
            .AddScoped<AppDelegate>()
            .AddScoped<CurrentProfileControl>()
            .AddScoped<SettingsControl>();
    }
}
