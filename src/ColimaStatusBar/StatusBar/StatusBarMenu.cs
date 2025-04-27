using Reactive;
using Reactive.Extensions;

namespace ColimaStatusBar.StatusBar;

public sealed class StatusBarMenu : IDisposable
{
    private readonly Reactive.IObserver<ColimaInteractor> observer;
    
    public StatusBarMenu(ColimaInteractor colimaInteractor)
    {
        observer = Observer.Create(colimaInteractor);

        Handle = new NSMenu();
        Handle.AddItem(new NSMenuItem());
        Handle.AddItem(NSMenuItem.SeparatorItem);

        var settingsItem = new NSMenuItem(
            "Launch at login",
            (_, _) => colimaInteractor.SetLaunchAtLogin(active: !colimaInteractor.IsLaunchedAtLogin))
        {
            State = NSCellStateValue.Off
        };

        Handle.AddItem(settingsItem);
        Handle.AddItem(new NSMenuItem("Quit", Quit));

        observer.Subscribe(static i => i.IsRunning)
            .Bind(Handle.Items.First(), static i => i.Title, static isRunning => isRunning ? "colima is running" : "colima is stopped")
            .Decorate(Handle.InvokeOnMainThread);
        
        observer.Subscribe(static i => i.Containers)
            .Handle(DisplayContainers)
            .Decorate(Handle.InvokeOnMainThread);
        
        observer.Subscribe(static i => i.IsLaunchedAtLogin)
            .Bind(settingsItem, static i => i.State, static launchAtLogin => launchAtLogin ? NSCellStateValue.On : NSCellStateValue.Off)
            .Decorate(Handle.InvokeOnMainThread);
        
        observer.Backfill();
    }

    public NSMenu Handle { get; }

    public void Dispose()
    {
        observer.Dispose();
        Handle.Dispose();
    }

    private void DisplayContainers(Container[] containers)
    {
        var currentContainers = Handle.Items.OfType<ContainerItem>().ToList();
        foreach (var container in currentContainers)
        {
            Handle.RemoveItem(container);
        }

        foreach (var container in containers)
        {
            Handle.InsertItem(new ContainerItem(container), index: 1);
        }
    }

    private static void Quit(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }

    private sealed class ContainerItem : NSMenuItem
    {
        public ContainerItem(Container container) : base(title: $"{container.Name}: {container.Image}", (_, _) => CopyToClipboard(container.Name))
        {
            ToolTip = "Copy container name";
        }
    }

    private static void CopyToClipboard(string text)
    {
        NSPasteboard.GeneralPasteboard.ClearContents();
        NSPasteboard.GeneralPasteboard.SetDataForType(NSData.FromString(text), NSPasteboardType.String.GetConstant());
    }
}
