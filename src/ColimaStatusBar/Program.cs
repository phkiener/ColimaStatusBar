using Avalonia;
using System;

namespace ColimaStatusBar;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var appBuilder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new MacOSPlatformOptions { ShowInDock = false, DisableDefaultApplicationMenuItems = true })
            .WithInterFont();

        return appBuilder.StartWithClassicDesktopLifetime(args);
    }
}
