using Swallow.Flux;

namespace ColimaStatusBar.Framework.AppKit;

public abstract class Control<TParent>(IDispatcher dispatcher, IBinder binder) where TParent : NSObject
{
    private TParent? attachedParent;
    
    public void Attach(TParent target)
    {
        attachedParent = target;
        OnAttach(target);
    }

    protected abstract void OnAttach(TParent target);

    protected TParent Parent => attachedParent ?? throw new InvalidOperationException("Cannot access parent when control is not attached");
    protected IDispatcher Dispatcher => dispatcher;
    protected IBinder Binder => binder;
}
