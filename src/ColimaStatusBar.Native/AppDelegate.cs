using System.ComponentModel;
using System.Linq.Expressions;

namespace ColimaStatusBar.Native;

public sealed class AppDelegate : NSApplicationDelegate
{
    private sealed record Subscription(object? Sender, string Property, Func<object, object> Receiver, Action<object> Handle);

    private readonly List<Subscription> subscriptions = [];
    private ColimaInteractor interactor = null!;
    private NSStatusItem? statusItem;
    private NSMenu? statusBarMenu;
    private NSMenuItem? status;

    public override void DidFinishLaunching(NSNotification notification)
    {
        interactor = new ColimaInteractor(TimeSpan.FromSeconds(1));
        interactor.PropertyChanged += Observe;
        Observe(interactor, i => i.IsRunning, UpdateStatus);
        
        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
        statusItem.Button.Image = NSImage.GetSystemSymbol("shippingbox", accessibilityDescription: null);
        
        statusBarMenu = new NSMenu();
        status = new NSMenuItem("Stopped");
        statusBarMenu.AddItem(status);

        var quit = new NSMenuItem("Quit", Quit);
        statusBarMenu.AddItem(quit);
        
        statusItem.Menu = statusBarMenu;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            interactor.PropertyChanged -= Observe;
            interactor.Dispose();
            
            statusBarMenu?.Dispose();
            statusItem?.Dispose();
        }
    }

    private void Observe(object? sender, PropertyChangedEventArgs e)
    {
        var matchingSubscriptions = subscriptions.Where(s => s.Sender == sender && s.Property == e.PropertyName).ToList();
        foreach (var matchingSubscription in matchingSubscriptions)
        {
            var value = matchingSubscription.Receiver(sender!);
            InvokeOnMainThread(() => matchingSubscription.Handle(value));
        }
    }

    private void Observe<T, TValue>(T sender, Expression<Func<T, TValue>> expression, Action<TValue> action)
    {
        var subscription = new Subscription(
            sender,
            ((MemberExpression)expression.Body).Member.Name,
            obj => expression.Compile()((T)obj)!,
            obj => action((TValue)obj));

        subscriptions.Add(subscription);
    }

    private void UpdateStatus(bool isRunning)
    {
        if (status is not null)
        {
            status.Title = isRunning ? "Running" : "Stopped";
        }

        if (statusItem is not null)
        {
            statusItem.Button.Image = NSImage.GetSystemSymbol(isRunning ? "shippingbox.fill" : "shippingbox", accessibilityDescription: null);
        }
    }

    private void Quit(object? sender, EventArgs args)
    {
        interactor.Quit();
    }
}
