using ColimaStatusBar.Core;

namespace ColimaStatusBar.StatusBar;

public sealed class RunningContainerItem : NSMenuItem
{
    public string ContainerId => Container.Id;
    
    public RunningContainer Container { get; }
    
    public RunningContainerItem(RunningContainer container)
    {
        Container = container;
        
        var menuItems = new List<NSMenuItem>
        {
            new($"Name: {container.Name}"),
            new($"Image: {container.Image}"),
            new($"Status: {container.State}"),
            SeparatorItem,
            new("Copy container name", (_, _) => CopyToClipboard(container.Name))
        };

        if (container.CanStart)
        {
            menuItems.Add(new NSMenuItem("Start", (_, _) => OnStart?.Invoke(this, EventArgs.Empty)));
        }

        if (container.CanStop)
        {
            menuItems.Add(new NSMenuItem("Stop", (_, _) => OnStop?.Invoke(this, EventArgs.Empty)));
        }

        if (container.CanRemove)
        {
            menuItems.Add(new NSMenuItem("Remove", (_, _) => OnRemove?.Invoke(this, EventArgs.Empty)));
        }
        

        Title = $"{container.Name}";
        Submenu = new NSMenu { Items = menuItems.ToArray() };
    }

    public event EventHandler? OnStart;
    public event EventHandler? OnStop;
    public event EventHandler? OnRemove;
    
    private static void CopyToClipboard(string text)
    {
        NSPasteboard.GeneralPasteboard.ClearContents();
        NSPasteboard.GeneralPasteboard.SetDataForType(NSData.FromString(text), NSPasteboardType.String.GetConstant());
    }
}
