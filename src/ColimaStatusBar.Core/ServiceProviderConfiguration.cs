using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Core.Colima;
using ColimaStatusBar.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Swallow.Flux;

namespace ColimaStatusBar.Core;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        return services
            .AddStore<IColima, ColimaStore>()
            .AddStore<ISettings, SettingsStore>();
    }
}
