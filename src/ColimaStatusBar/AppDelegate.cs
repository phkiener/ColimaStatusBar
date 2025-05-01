using ColimaStatusBar.StatusBar;
using Microsoft.Extensions.DependencyInjection;

namespace ColimaStatusBar;

public sealed class AppDelegate(IServiceProvider serviceProvider) : NSApplicationDelegate
{
    private StatusBarIcon? statusBarIcon;
    private StatusBarMenu? statusBarMenu;

    public override void DidFinishLaunching(NSNotification notification)
    {
        statusBarIcon = ActivatorUtilities.CreateInstance<StatusBarIcon>(serviceProvider, NSStatusBar.SystemStatusBar);
        statusBarMenu = ActivatorUtilities.CreateInstance<StatusBarMenu>(serviceProvider);
        
        statusBarIcon.Handle.Menu = statusBarMenu.Handle;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            statusBarMenu?.Dispose();
            statusBarIcon?.Dispose();
        }
    }
}
