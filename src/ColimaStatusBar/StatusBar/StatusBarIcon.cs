using Reactive;
using Reactive.Extensions;

namespace ColimaStatusBar.StatusBar;

public sealed class StatusBarIcon : IDisposable
{
    private readonly Reactive.IObserver<ColimaInteractor> observer;

    public StatusBarIcon(NSStatusBar statusBar, ColimaInteractor interactor)
    {
        Handle = statusBar.CreateStatusItem(NSStatusItemLength.Square);
        observer = Observer.Create(interactor);

        observer.Subscribe(i => i.IsRunning)
            .Bind(Handle.Button, static btn => btn.Image, GetStatusImage)
            .Decorate(Handle.InvokeOnMainThread);

        observer.Backfill();
    }

    public NSStatusItem Handle { get; }

    private static NSImage? GetStatusImage(bool isRunning)
    {
        return isRunning ? NSImage.GetSystemSymbol("shippingbox.fill", null) : NSImage.GetSystemSymbol("shippingbox", null);
    }

    public void Dispose()
    {
        Handle.Dispose();
        observer.Dispose();
    }
}
