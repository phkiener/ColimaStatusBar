using System.ComponentModel;

namespace Reactive;

public interface ISubscription<TObservable, out TValue> where TObservable : INotifyPropertyChanged
{
    ISubscription<TObservable, TValue> Handle(Action<TValue> handler);
    ISubscription<TObservable, TValue> Decorate(Action<Action> decorator);
}
