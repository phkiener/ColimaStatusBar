using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class LaunchAtLoginItem : NSMenuItem
{
    private readonly Dispatcher dispatcher;
    private readonly Emitter emitter;
    private readonly SettingsStore settings;

    public LaunchAtLoginItem(Dispatcher dispatcher, Emitter emitter, SettingsStore settings)
    {
        this.dispatcher = dispatcher;
        this.emitter = emitter;
        this.settings = settings;
        
        Draw();
        Activated += OnClick;
        emitter.OnEmit += OnEmit;
    }

    private void Draw()
    {
        Title = "Launch at Login";
        State = settings.StartAtLogin ? NSCellStateValue.On : NSCellStateValue.Off;
    }

    private void OnClick(object? sender, EventArgs eventArgs)
    {
        _ = dispatcher.Invoke(new Commands.LaunchAtLogin(!settings.StartAtLogin));
    }

    private void OnEmit(object? sender, INotification notification)
    {
        if (notification is LaunchAtLoginChanged)
        {
            InvokeOnMainThread(Draw);
        }
    }

    protected override void Dispose(bool disposing)
    {
        Activated -= OnClick;
        emitter.OnEmit -= OnEmit;

        base.Dispose(disposing);
    }
}
