using ColimaStatusBar.Core;

namespace ColimaStatusBar.StatusBar;

public sealed class RunningContainerItem : NSMenuItem
{
    private readonly RunningContainer container;

    public RunningContainerItem(RunningContainer container)
    {
        this.container = container;

        Draw();
    }

    private void Draw()
    {
        Title = $"{container.Name}";
        Submenu = new NSMenu
        {
            Items =
            [
                new NSMenuItem($"Name: {container.Name}"),
                new NSMenuItem($"Image: {container.Image}"),
                new NSMenuItem($"Status: {container.State}")
            ]
        };
    }
}
