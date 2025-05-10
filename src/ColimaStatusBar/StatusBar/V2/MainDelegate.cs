using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using ColimaStatusBar.Framework.Flux;
using ColimaStatusBar.StatusBar.V2;

namespace ColimaStatusBar;

public sealed class MainDelegate(
    ColimaStatusStore store,
    CurrentProfileControl currentProfileControl,
    RunningContainersControl runningContainersControl,
    SettingsControl settingsControl,
    Binder binder) : NSApplicationDelegate
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
            binder.Dispose();
            currentProfileControl.Dispose();
            runningContainersControl.Dispose();
            settingsControl.Dispose();

            statusItem.Menu.Dispose();
            statusItem.Dispose();
            
        }
    }
}
