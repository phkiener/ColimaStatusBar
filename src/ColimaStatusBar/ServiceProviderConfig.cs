using ColimaStatusBar.Core;
using ColimaStatusBar.Framework;
using ColimaStatusBar.Framework.Flux;
using ColimaStatusBar.StatusBar.V2;
using Microsoft.Extensions.DependencyInjection;

namespace ColimaStatusBar;

public static class ServiceProviderConfig
{
    public static AsyncServiceScope BuildServiceScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddFramework()
            .AddCore()
            .AddStatusBar()
            .BuildServiceProvider();

        return serviceProvider.CreateAsyncScope();
    }

    public static async Task Dispatch<TCommand>(this AsyncServiceScope scope) where TCommand : ICommand, new()
    {
        var dispatcher = scope.ServiceProvider.GetRequiredService<Dispatcher>();
        await dispatcher.Invoke<TCommand>();
    }
}
