using ColimaStatusBar.Core.Platform;
using ColimaStatusBar.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace ColimaStatusBar.Core;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddPlatform(this IServiceCollection services)
    {
        return services
            .AddSingleton<ILoginItemsService, LoginItemsService>()
            .AddSingleton<IShellExecutor, ShellExecutor>();
    }
}
