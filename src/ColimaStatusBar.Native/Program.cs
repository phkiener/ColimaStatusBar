using ColimaStatusBar.Native;

NSApplication.Init();
NSApplication.SharedApplication.Delegate = new AppDelegate();
NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;
NSApplication.SharedApplication.Run();
