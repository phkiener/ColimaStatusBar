using System.ComponentModel;

namespace Reactive.Implementation;

internal sealed class ChangeHandler<TObservable>(Func<TObservable, object?> valueFunc)
    where TObservable : INotifyPropertyChanged
{
    private readonly List<Action<object?>> handlers = [];
    private Action<Action> actionDecorator = static a => a();

    public void Handle(TObservable observable)
    {
        var value = valueFunc(observable);
        foreach (var handler in handlers)
        {
            actionDecorator.Invoke(() => handler(value));
        }
    }
    
    public sealed class Configurator<TValue>(ChangeHandler<TObservable> changeHandler) : ISubscription<TObservable, TValue>
    {
        public ISubscription<TObservable, TValue> Handle(Action<TValue> handler)
        {
            changeHandler.handlers.Add(v => handler((TValue)v!));
            
            return this;
        }

        public ISubscription<TObservable, TValue> Decorate(Action<Action> decorator)
        {
            changeHandler.actionDecorator = decorator;

            return this;
        }
    }
}
