using ColimaStatusBar.Core;
using ColimaStatusBar.StatusBar;
using Microsoft.Extensions.DependencyInjection;
using Swallow.Flux;

namespace ColimaStatusBar;

public static class ServiceProviderConfig
{
    public static AsyncServiceScope BuildServiceScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddFlux()
            .AddCore()
            .AddStatusBar()
            .BuildServiceProvider();

        return serviceProvider.CreateAsyncScope();
    }

    public static async Task Dispatch<TCommand>(this AsyncServiceScope scope) where TCommand : ICommand, new()
    {
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();
        await dispatcher.Dispatch<TCommand>();
    }
}
