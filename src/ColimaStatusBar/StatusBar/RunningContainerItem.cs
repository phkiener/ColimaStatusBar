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
        Title = $"{container.Name} {container.State}";
        Menu = new NSMenu
        {
            Items =
            [
                new NSMenuItem($"Name: {container.Name}") { Enabled = false },
                new NSMenuItem($"Image: {container.Image}") { Enabled = false }
            ]
        };
    }
}
