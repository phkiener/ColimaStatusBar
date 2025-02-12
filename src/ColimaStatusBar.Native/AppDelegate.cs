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
    private NSMenuItem[] containerItems = [];

    public override void DidFinishLaunching(NSNotification notification)
    {
        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
        statusItem.Button.Image = NSImage.GetSystemSymbol("shippingbox", accessibilityDescription: null);

        statusBarMenu = new NSMenu();
        status = new NSMenuItem("");
        statusBarMenu.AddItem(status);
        
        statusBarMenu.AddItem(NSMenuItem.SeparatorItem);
        var quit = new NSMenuItem("Quit", Quit);
        statusBarMenu.AddItem(quit);
        
        statusItem.Menu = statusBarMenu;

        interactor = new ColimaInteractor(TimeSpan.FromSeconds(1));
        observer = Observer.Create(interactor);
        observer.Subscribe(i => i.IsRunning)
            .Bind(status, s => s.Title, static isRunning => isRunning ? "colima is running" : "Colima is stopped")
            .Bind(statusItem.Button, b => b.Image, static isRunning => NSImage.GetSystemSymbol(isRunning ? "shippingbox.fill" : "shippingbox", accessibilityDescription: null))
            .Decorate(InvokeOnMainThread);

        observer.Subscribe(i => i.Containers)
            .Handle(DisplayContainers)
            .Decorate(InvokeOnMainThread);
        
        observer.Backfill();
    }

    private void DisplayContainers(Container[] containers)
    {
        if (statusBarMenu is null)
        {
            return;
        }
        
        foreach (var menuItem in containerItems)
        {
            statusBarMenu.RemoveItem(menuItem);
        }

        containerItems = containers.Select(static c => new NSMenuItem($"{c.Name}: {c.Image}")).ToArray();
        
        var items = new List<NSMenuItem>(statusBarMenu.Items);
        items.InsertRange(1, containerItems);
        statusBarMenu.Items = items.ToArray();
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
