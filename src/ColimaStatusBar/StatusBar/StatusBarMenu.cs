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
        Handle.AddItem(new NSMenuItem("Quit", Quit));

        observer.Subscribe(static i => i.IsRunning)
            .Bind(Handle.Items.First(), static i => i.Title, static isRunning => isRunning ? "colima is running" : "colima is stopped")
            .Decorate(Handle.InvokeOnMainThread);
        
        observer.Subscribe(static i => i.Containers)
            .Handle(DisplayContainers)
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
        private readonly Container container;

        public ContainerItem(Container container) : base(title: $"{container.Name}: {container.Image}", HandleClick)
        {
            this.container = container;
            
            ToolTip = "Copy container name";
        }

        private static void HandleClick(object? sender, EventArgs e)
        {
            var containerItem = (ContainerItem)sender!;
            var containerName = containerItem.container.Name;
            
            NSPasteboard.GeneralPasteboard.ClearContents();
            NSPasteboard.GeneralPasteboard.SetDataForType(NSData.FromString(containerName), NSPasteboardType.String.GetConstant());
        }
    }
}
