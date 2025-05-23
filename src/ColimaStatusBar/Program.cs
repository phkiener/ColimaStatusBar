﻿using ColimaStatusBar;
using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Ui;
using Microsoft.Extensions.DependencyInjection;

await using var scope = ServiceProviderConfig.BuildServiceScope();
await scope.Dispatch<Commands.Initialize>();

NSApplication.Init();
NSApplication.SharedApplication.Delegate = scope.ServiceProvider.GetRequiredService<AppDelegate>();
NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;
NSApplication.SharedApplication.Run();

await scope.Dispatch<Commands.Shutdown>();
