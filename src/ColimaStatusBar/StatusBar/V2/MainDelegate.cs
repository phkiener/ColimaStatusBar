using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using ColimaStatusBar.Framework.Flux;
using ColimaStatusBar.StatusBar.V2;

namespace ColimaStatusBar;

public sealed class MainDelegate(ColimaStatusStore store, CurrentProfile currentProfile, Binder binder) : NSApplicationDelegate
{
    private NSStatusItem? statusItem;
    
    public override void DidFinishLaunching(NSNotification notification)
    {
        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);
        statusItem.Menu = new NSMenu();

        binder.BindControl(statusItem).To<ColimaStatusChanged>(SetStatusImage);
        currentProfile.Attach(statusItem.Menu);
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
            currentProfile.Dispose();

            statusItem.Menu.Dispose();
            statusItem.Dispose();
            
        }
    }
}
