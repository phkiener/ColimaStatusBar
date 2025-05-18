using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Ui.Controls;
using ColimaStatusBar.Ui.Framework;
using Swallow.Flux;

namespace ColimaStatusBar.Ui;

public sealed class AppDelegate(
    IColima colima,
    CurrentProfileControl currentProfileControl,
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
        
        // runningContainersControl.Attach(statusItem.Menu);
        // statusItem.Menu.AddItem(NSMenuItem.SeparatorItem);
        
        settingsControl.Attach(statusItem.Menu);

        binder.BindControl(statusItem).To<ProfileStatusChanged>(SetStatusImage, immediatelyInvoke: true);
    }

    private void SetStatusImage(NSStatusItem item)
    {
        item.Button.Image = colima.OverallStatus switch
        {
            ProfileStatus.Running => NSImage.GetSystemSymbol("shippingbox.fill", null),
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
