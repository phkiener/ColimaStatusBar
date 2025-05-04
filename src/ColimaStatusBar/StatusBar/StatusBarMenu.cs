using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class StatusBarMenu : IDisposable
{
    private readonly Emitter emitter;
    private readonly RunningContainersStore runningContainers;

    public StatusBarMenu(Dispatcher dispatcher, Emitter emitter, ColimaStatusStore colimaStatus, RunningContainersStore runningContainers, SettingsStore settingsStore)
    {
        this.emitter = emitter;
        this.runningContainers = runningContainers;
        
        Handle = new NSMenu();
        Handle.AddItem(new CurrentStatusItem(dispatcher, emitter, colimaStatus));
        Handle.AddItem(new CurrentProfileItem(emitter, colimaStatus));
        Handle.AddItem(NSMenuItem.SeparatorItem);
        Handle.AddItem(new LaunchAtLoginItem(dispatcher, emitter, settingsStore));
        Handle.AddItem(new NSMenuItem(title: "Quit", Quit));

        emitter.OnEmit += HandleNotification;
    }

    private void HandleNotification(object? sender, INotification notification)
    {
        if (notification is RunningContainersChanged)
        {
            Handle.InvokeOnMainThread(DisplayContainers);
        }
    }

    public NSMenu Handle { get; }

    private void DisplayContainers()
    {
        var currentContainers = Handle.Items.OfType<RunningContainerItem>().ToList();
        foreach (var container in currentContainers)
        {
            Handle.RemoveItem(container);
        }

        var extraSeparator = Handle.Items.Where(static i => i.IsSeparatorItem).Skip(1).FirstOrDefault();
        if (extraSeparator is not null)
        {
            Handle.RemoveItem(extraSeparator);
        }

        if (runningContainers.RunningContainers.Any())
        {
            Handle.InsertItem(NSMenuItem.SeparatorItem, index: 3);
        }

        foreach (var container in runningContainers.RunningContainers.Reverse())
        {
            Handle.InsertItem(new RunningContainerItem(container), index: 3);
        }
    }

    private void Quit(object? sender, EventArgs e)
    {
        NSApplication.SharedApplication.Stop(Handle);
    }

    public void Dispose()
    {
        emitter.OnEmit -= HandleNotification;
        Handle.Dispose();
    }
}
