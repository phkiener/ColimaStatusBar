using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.Framework.AppKit;

public static class BinderExtensions
{
    public static ITargetedBinding<T> BindControl<T>(this Binder binder, T target) where T : NSObject
    {
        return binder.Bind(target, target.InvokeOnMainThread);
    }
}
