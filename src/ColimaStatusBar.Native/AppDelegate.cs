using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

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
        
        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
        statusItem.Button.Image = NSImage.GetSystemSymbol("shippingbox", accessibilityDescription: null);
        
        statusBarMenu = new NSMenu();
        status = new NSMenuItem("Stopped");
        statusBarMenu.AddItem(status);

        var quit = new NSMenuItem("Quit", Quit);
        statusBarMenu.AddItem(quit);
        
        statusItem.Menu = statusBarMenu;

        interactor = new ColimaInteractor(TimeSpan.FromSeconds(1));
        interactor.PropertyChanged += Observe;
        Binding(status, s => s.Title, interactor, i => i.IsRunning, static b => b ? "Running" : "Stopped");
        Binding(statusItem.Button, s => s.Image, interactor, i => i.IsRunning, static b => NSImage.GetSystemSymbol(b ? "shippingbox.fill" : "shippingbox", accessibilityDescription: null));
    }

    private void Binding<T, TValue, T2, TValue2>(T target, Expression<Func<T, TValue>> targetProperty, T2 source, Expression<Func<T2, TValue2>> sourceProperty, Func<TValue2, TValue> convert)
    {
        var subscription = new Subscription(
            source,
            ((MemberExpression)sourceProperty.Body).Member.Name,
            obj => sourceProperty.Compile()((T2)obj)!,
            obj => (((MemberExpression)targetProperty.Body).Member as PropertyInfo)!.SetValue(target, convert((TValue2)obj)));

        subscriptions.Add(subscription);
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

    private void Quit(object? sender, EventArgs args)
    {
        interactor.Quit();
    }
}
