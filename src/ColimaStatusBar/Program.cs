using ColimaStatusBar;
using ColimaStatusBar.Core;
using ColimaStatusBar.Framework;
using ColimaStatusBar.Framework.Flux;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
    .AddFramework()
    .AddStore<ColimaStatusStore>()
    .AddStore<RunningContainersStore>()
    .BuildServiceProvider();

await using var scope = serviceProvider.CreateAsyncScope();
var dispatcher = scope.ServiceProvider.GetRequiredService<Dispatcher>();
await dispatcher.Invoke<Commands.Initialize>();

using var colimaInteractor = new ColimaInteractor(TimeSpan.FromSeconds(5));

NSApplication.Init();
NSApplication.SharedApplication.Delegate = new AppDelegate(colimaInteractor);
NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;
NSApplication.SharedApplication.Run();

await dispatcher.Invoke<Commands.Shutdown>();
