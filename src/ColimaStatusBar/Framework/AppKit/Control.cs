using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.Framework.AppKit;

public abstract class Control<TParent>(Dispatcher dispatcher, Binder binder) : IDisposable where TParent : NSObject
{
    public void Attach(TParent target)
    {
        OnAttach(target);
    }

    protected abstract void OnAttach(TParent target);

    protected Dispatcher Dispatcher => dispatcher;
    protected Binder Binder => binder;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        binder.Dispose();
    }
}
