using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.Framework.AppKit;

public abstract class Control<TParent>(Dispatcher dispatcher, Binder binder) : IDisposable where TParent : NSObject
{
    private TParent? attachedParent;
    
    public void Attach(TParent target)
    {
        attachedParent = target;
        OnAttach(target);
    }

    protected abstract void OnAttach(TParent target);

    protected TParent Parent => attachedParent ?? throw new InvalidOperationException("Cannot access parent when control is not attached");
    protected Dispatcher Dispatcher => dispatcher;
    protected Binder Binder => binder;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        binder.Dispose();
    }
}
