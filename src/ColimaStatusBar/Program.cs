using ColimaStatusBar;
using ColimaStatusBar.Core;
using ColimaStatusBar.Framework;
using ColimaStatusBar.Framework.Flux;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
    .AddFramework()
    .AddStore<ColimaStatusStore>()
    .BuildServiceProvider();

await using var scope = serviceProvider.CreateAsyncScope();
var dispatcher = scope.ServiceProvider.GetRequiredService<Dispatcher>();
var emitter = scope.ServiceProvider.GetRequiredService<Emitter>();
var store = scope.ServiceProvider.GetRequiredService<ColimaStatusStore>();

emitter.OnEmit += (_, n) =>
{
    Console.WriteLine($"Notification received: {n.GetType().Name}");
    Console.WriteLine($"Current status: {store.CurrentStatus}");
};

await dispatcher.Invoke<Commands.Initialize>();

using var colimaInteractor = new ColimaInteractor(TimeSpan.FromSeconds(5));

NSApplication.Init();
NSApplication.SharedApplication.Delegate = new AppDelegate(colimaInteractor);
NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;
NSApplication.SharedApplication.Run();

await dispatcher.Invoke<Commands.Shutdown>();
