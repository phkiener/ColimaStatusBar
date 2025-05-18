using Swallow.Flux;

namespace ColimaStatusBar.Ui.Framework;

public static class BinderExtensions
{
    public static ITargetedBinding<T> BindControl<T>(this IBinder binder, T target) where T : NSObject
    {
        return binder.Bind(target, target.InvokeOnMainThread);
    }
}
