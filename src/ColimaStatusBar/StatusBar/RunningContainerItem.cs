using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class RunningContainerItem : NSMenuItem
{
    private readonly Dispatcher dispatcher;
    private readonly RunningContainer container;

    public RunningContainerItem(Dispatcher dispatcher, RunningContainer container)
    {
        this.dispatcher = dispatcher;
        this.container = container;

        Draw();
    }

    private void Draw()
    {
        var menuItems = new List<NSMenuItem>
        {
            new($"Name: {container.Name}", (_, _) => CopyToClipboard(container.Name)) { ToolTip = "Copy container name" },
            new($"Image: {container.Image}"),
            new($"Status: {container.State}")
        };

        if (container.CanStart || container.CanStop || container.CanRemove)
        {
            menuItems.Add(SeparatorItem);
        }

        if (container.CanStart)
        {
            menuItems.Add(new NSMenuItem("Start", (_, _) => _ =dispatcher.Invoke(new Commands.StartContainer(container.Id))));
        }

        if (container.CanStop)
        {
            menuItems.Add(new NSMenuItem("Stop", (_, _) => _ =dispatcher.Invoke(new Commands.StopContainer(container.Id))));
        }

        if (container.CanRemove)
        {
            menuItems.Add(new NSMenuItem("Remove", (_, _) => _ =dispatcher.Invoke(new Commands.RemoveContainer(container.Id))));
        }

        Title = $"{container.Name}";
        Submenu = new NSMenu { Items = menuItems.ToArray() };
    }

    private static void CopyToClipboard(string text)
    {
        NSPasteboard.GeneralPasteboard.ClearContents();
        NSPasteboard.GeneralPasteboard.SetDataForType(NSData.FromString(text), NSPasteboardType.String.GetConstant());
    }
}
