using Reactive;
using Reactive.Extensions;

namespace ColimaStatusBar.Native;

public sealed class AppDelegate : NSApplicationDelegate
{
    private Reactive.IObserver<ColimaInteractor>? observer;
    private ColimaInteractor interactor = null!;
    private NSStatusItem? statusItem;
    private NSMenu? statusBarMenu;
    private NSMenuItem? status;

    public override void DidFinishLaunching(NSNotification notification)
    {
        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
        statusItem.Button.Image = NSImage.GetSystemSymbol("shippingbox", accessibilityDescription: null);

        statusBarMenu = new NSMenu();
        status = new NSMenuItem("Stopped");
        statusBarMenu.AddItem(status);

        var quit = new NSMenuItem("Quit", Quit);
        statusBarMenu.AddItem(quit);
        
        statusItem.Menu = statusBarMenu;

        interactor = new ColimaInteractor(TimeSpan.FromSeconds(1));
        observer = Observer.Create(interactor);
        observer.Subscribe(i => i.IsRunning)
            .Bind(status, s => s.Title, static isRunning => isRunning ? "Running" : "Stopped")
            .Bind(statusItem.Button, b => b.Image, static isRunning => NSImage.GetSystemSymbol(isRunning ? "shippingbox.fill" : "shippingbox", accessibilityDescription: null))
            .Decorate(InvokeOnMainThread);
        
        observer.Backfill();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            observer?.Dispose();
            interactor.Dispose();
            
            statusBarMenu?.Dispose();
            statusItem?.Dispose();
        }
    }

    private void Quit(object? sender, EventArgs args)
    {
        interactor.Quit();
    }
}
