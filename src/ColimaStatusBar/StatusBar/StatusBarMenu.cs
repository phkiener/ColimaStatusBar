using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class StatusBarMenu : IDisposable
{
    private readonly Dispatcher dispatcher;
    private readonly Emitter emitter;
    private readonly ColimaStatusStore colimaStatus;
    private readonly RunningContainersStore runningContainers;
    private readonly SettingsStore settingsStore;

    public StatusBarMenu(Dispatcher dispatcher, Emitter emitter, ColimaStatusStore colimaStatus, RunningContainersStore runningContainers, SettingsStore settingsStore)
    {
        this.dispatcher = dispatcher;
        this.emitter = emitter;
        this.colimaStatus = colimaStatus;
        this.runningContainers = runningContainers;
        this.settingsStore = settingsStore;

        Handle = new NSMenu();
        Handle.AddItem(new NSMenuItem(title: colimaStatus.CurrentStatus is ColimaStatus.Running ? "colima is running" : "colima is stopped"));
        Handle.AddItem(NSMenuItem.SeparatorItem);
        // Running containers will be placed between those separators
        Handle.AddItem(NSMenuItem.SeparatorItem);
        Handle.AddItem(new NSMenuItem("Launch at login", ToggleLaunchAtLogin) { State = NSCellStateValue.Off });
        Handle.AddItem(new NSMenuItem("Quit", Quit));

        emitter.OnEmit += HandleNotification;
    }

    private void HandleNotification(object? sender, INotification notification)
    {
        if (notification is ColimaStatusChanged)
        {
            Handle.InvokeOnMainThread(() => Handle.Items[0].Title = colimaStatus.CurrentStatus is ColimaStatus.Running ? "colima is running" : "colima is stopped");
        }

        if (notification is LaunchAtLoginChanged)
        {
            Handle.InvokeOnMainThread(() => Handle.Items[^2].State = settingsStore.StartAtLogin ? NSCellStateValue.On : NSCellStateValue.Off);
        }

        if (notification is RunningContainersChanged)
        {
            Handle.InvokeOnMainThread(DisplayContainers);
        }
    }

    public NSMenu Handle { get; }

    private void DisplayContainers()
    {
        var currentContainers = Handle.Items.OfType<ContainerItem>().ToList();
        foreach (var container in currentContainers)
        {
            Handle.RemoveItem(container);
        }

        foreach (var container in runningContainers.RunningContainers)
        {
            Handle.InsertItem(new ContainerItem(container), index: 2);
        }
    }

    private void ToggleLaunchAtLogin(object? sender, EventArgs e)
    {
        _ = dispatcher.Invoke(new Commands.LaunchAtLogin(!settingsStore.StartAtLogin));
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

    private sealed class ContainerItem : NSMenuItem
    {
        public ContainerItem(RunningContainer container) : base(title: $"{container.Name}: {container.Image}", (_, _) => CopyToClipboard(container.Name))
        {
            ToolTip = "Copy container name";
            State = container.State is ContainerState.Running ? NSCellStateValue.On : NSCellStateValue.Off;
        }
    }

    private static void CopyToClipboard(string text)
    {
        NSPasteboard.GeneralPasteboard.ClearContents();
        NSPasteboard.GeneralPasteboard.SetDataForType(NSData.FromString(text), NSPasteboardType.String.GetConstant());
    }
}
