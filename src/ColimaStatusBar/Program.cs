using ColimaStatusBar;

using var colimaInteractor = new ColimaInteractor(TimeSpan.FromSeconds(5));

NSApplication.Init();
NSApplication.SharedApplication.Delegate = new AppDelegate(colimaInteractor);
NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;
NSApplication.SharedApplication.Run();
