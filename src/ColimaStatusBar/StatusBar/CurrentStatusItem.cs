using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class CurrentStatusItem : NSMenuItem
{
    private readonly Dispatcher dispatcher;
    private readonly Emitter emitter;
    private readonly ColimaStatusStore colimaStatus;

    public CurrentStatusItem(Dispatcher dispatcher, Emitter emitter, ColimaStatusStore colimaStatus)
    {
        this.dispatcher = dispatcher;
        this.emitter = emitter;
        this.colimaStatus = colimaStatus;
        
        Draw();
        Activated += OnClick;
        emitter.OnEmit += OnEmit;
    }

    private void Draw()
    {
        Title = colimaStatus.CurrentStatus switch
        {
            ColimaStatus.Stopped => "Colima is stopped",
            ColimaStatus.Starting => "Colima is starting...",
            ColimaStatus.Running => "Colima is running",
            ColimaStatus.Stopping => "Colima is stopping...",
            _ => ""
        };
        
        ToolTip = colimaStatus.CurrentStatus switch
        {
            ColimaStatus.Stopped => "Start colima",
            ColimaStatus.Running => "Stop colima",
            _ => null
        };
    }

    private void OnClick(object? sender, EventArgs eventArgs)
    {
        if (colimaStatus.CurrentStatus is ColimaStatus.Running)
        {
            _ = dispatcher.Invoke<Commands.StopColima>();
        }
        else if (colimaStatus.CurrentStatus is ColimaStatus.Stopped)
        {
            _ = dispatcher.Invoke<Commands.StartColima>();
        }
    }

    private void OnEmit(object? sender, INotification notification)
    {
        if (notification is ColimaStatusChanged)
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
