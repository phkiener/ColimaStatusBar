using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar.V2;

public sealed class SettingsControl(SettingsStore store, Dispatcher dispatcher, Binder binder) : Control<NSMenu>(dispatcher, binder)
{
    private readonly NSMenuItem laucnhAtLoginItem = new("Launch at login");
    private readonly NSMenuItem quitItem = new("Quit");
    
    protected override void OnAttach(NSMenu target)
    {
        target.AddItem(laucnhAtLoginItem);
        target.AddItem(quitItem);

        Binder.BindControl(laucnhAtLoginItem).To<LaunchAtLoginChanged>(UpdateCellState);
        laucnhAtLoginItem.Activated += OnLaunchAtLoginClicked;
        quitItem.Activated += OnQuitClicked;
    }

    private void UpdateCellState(NSMenuItem item)
    {
        item.State = store.StartAtLogin ? NSCellStateValue.On : NSCellStateValue.Off;
    }

    private async void OnLaunchAtLoginClicked(object? sender, EventArgs args)
    {
        await Dispatcher.Invoke(new Commands.LaunchAtLogin(!store.StartAtLogin));
    }
    
    private void OnQuitClicked(object? sender, EventArgs args)
    {
        NSApplication.SharedApplication.Stop(Parent);
    }
}
