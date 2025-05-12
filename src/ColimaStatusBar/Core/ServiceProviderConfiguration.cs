using Microsoft.Extensions.DependencyInjection;
using Swallow.Flux;

namespace ColimaStatusBar.Core;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        return services.AddStore<ColimaStatusStore>()
            .AddStore<RunningContainersStore>()
            .AddStore<SettingsStore>();
    }
}
