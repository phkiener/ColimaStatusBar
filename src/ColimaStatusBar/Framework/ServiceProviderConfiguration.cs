using ColimaStatusBar.Framework.Flux;
using Microsoft.Extensions.DependencyInjection;

namespace ColimaStatusBar.Framework;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddStore<TStore>(this IServiceCollection services) where TStore : class, IStore
    {
        return services.AddScoped<TStore>()
            .AddScoped<IStore>(static sp => sp.GetRequiredService<TStore>());
    }

    public static IServiceCollection AddFramework(this IServiceCollection services)
    {
        return services.AddScoped<Dispatcher>()
            .AddScoped<Emitter>()
            .AddTransient<Binder>();
    }
}
