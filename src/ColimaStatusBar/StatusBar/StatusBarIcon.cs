using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.Flux;
namespace ColimaStatusBar.StatusBar;

public sealed class StatusBarIcon : IDisposable
{
    private readonly Emitter emitter;
    private readonly ColimaStatusStore colimaStatus;

    public StatusBarIcon(NSStatusBar statusBar, Emitter emitter, ColimaStatusStore colimaStatus)
    {
        this.emitter = emitter;
        this.colimaStatus = colimaStatus;
        
        Handle = statusBar.CreateStatusItem(NSStatusItemLength.Square);
        SetStatusImage();

        emitter.OnEmit += HandleNotification;
    }

    private void HandleNotification(object? sender, INotification notification)
    {
        if (notification is ColimaStatusChanged)
        {
            Handle.InvokeOnMainThread(SetStatusImage);
        }
    }

    public NSStatusItem Handle { get; }

    private void SetStatusImage()
    {
        Handle.Button.Image = colimaStatus.CurrentStatus is ColimaStatus.Running
            ? NSImage.GetSystemSymbol("shippingbox.fill", null)
            : NSImage.GetSystemSymbol("shippingbox", null);
    }

    public void Dispose()
    {
        emitter.OnEmit -= HandleNotification;
        Handle.Dispose();
    }
}
