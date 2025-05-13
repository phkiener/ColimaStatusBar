using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using Swallow.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class AppDelegate(
    ColimaStatusStore store,
    CurrentProfileControl currentProfileControl,
    RunningContainersControl runningContainersControl,
    SettingsControl settingsControl,
    IBinder binder) : NSApplicationDelegate
{
    private NSStatusItem? statusItem;
    
    public override void DidFinishLaunching(NSNotification notification)
    {
        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);
        statusItem.Menu = new NSMenu();

        currentProfileControl.Attach(statusItem.Menu);
        statusItem.Menu.AddItem(NSMenuItem.SeparatorItem);
        runningContainersControl.Attach(statusItem.Menu);
        statusItem.Menu.AddItem(NSMenuItem.SeparatorItem);
        settingsControl.Attach(statusItem.Menu);

        SetStatusImage(statusItem);
        binder.BindControl(statusItem).To<ColimaStatusChanged>(SetStatusImage);
    }

    private void SetStatusImage(NSStatusItem item)
    {
        item.Button.Image = store.CurrentStatus switch
        {
            ColimaStatus.Running => NSImage.GetSystemSymbol("shippingbox.fill", null),
            _ => NSImage.GetSystemSymbol("shippingbox", null)
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && statusItem?.Menu is not null)
        {
            statusItem.Menu.Dispose();
            statusItem.Dispose();
            
        }
    }
}
