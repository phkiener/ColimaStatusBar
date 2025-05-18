using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Ui.Framework;
using Swallow.Flux;

namespace ColimaStatusBar.Ui.Controls;

public sealed class SettingsControl(ISettings settings, IDispatcher dispatcher, IBinder binder) : Control<NSMenu>(dispatcher, binder)
{
    private readonly NSMenuItem launchAtLoginItem = new("Launch at login");
    private readonly NSMenuItem quitItem = new("Quit");
    
    protected override void OnAttach(NSMenu target)
    {
        target.AddItem(launchAtLoginItem);
        target.AddItem(quitItem);

        Binder.BindControl(launchAtLoginItem).To<LaunchAtLoginChanged>(UpdateCellState, immediatelyInvoke: true);

        launchAtLoginItem.Activated += OnLaunchAtLoginClicked;
        quitItem.Activated += OnQuitClicked;
    }

    private void UpdateCellState(NSMenuItem item)
    {
        item.State = settings.LaunchAtLogin ? NSCellStateValue.On : NSCellStateValue.Off;
    }

    private async void OnLaunchAtLoginClicked(object? sender, EventArgs args)
    {
        var toggledState = !settings.LaunchAtLogin;
        await Dispatcher.Dispatch(new Commands.LaunchAtLogin(toggledState));
    }
    
    private void OnQuitClicked(object? sender, EventArgs args)
    {
        NSApplication.SharedApplication.Stop(Parent);
    }
}
