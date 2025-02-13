using ColimaStatusBar.StatusBar;

namespace ColimaStatusBar;

public sealed class AppDelegate(ColimaInteractor colimaInteractor) : NSApplicationDelegate
{
    private StatusBarIcon? statusBarIcon;
    private StatusBarMenu? statusBarMenu;

    public override void DidFinishLaunching(NSNotification notification)
    {
        statusBarIcon = new StatusBarIcon(NSStatusBar.SystemStatusBar, colimaInteractor);
        statusBarMenu = new StatusBarMenu(colimaInteractor);
        
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
