using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using Swallow.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class SettingsControl(SettingsStore store, IDispatcher dispatcher, IBinder binder) : Control<NSMenu>(dispatcher, binder)
{
    private readonly NSMenuItem launchAtLoginItem = new("Launch at login");
    private readonly NSMenuItem quitItem = new("Quit");
    
    protected override void OnAttach(NSMenu target)
    {
        target.AddItem(launchAtLoginItem);
        target.AddItem(quitItem);

        UpdateCellState(launchAtLoginItem);
        Binder.BindControl(launchAtLoginItem).To<LaunchAtLoginChanged>(UpdateCellState);

        launchAtLoginItem.Activated += OnLaunchAtLoginClicked;
        quitItem.Activated += OnQuitClicked;
    }

    private void UpdateCellState(NSMenuItem item)
    {
        item.State = store.StartAtLogin ? NSCellStateValue.On : NSCellStateValue.Off;
    }

    private async void OnLaunchAtLoginClicked(object? sender, EventArgs args)
    {
        await Dispatcher.Dispatch(new Commands.LaunchAtLogin(!store.StartAtLogin));
    }
    
    private void OnQuitClicked(object? sender, EventArgs args)
    {
        NSApplication.SharedApplication.Stop(Parent);
    }
}
