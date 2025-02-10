using System.ComponentModel;

namespace Reactive;

public interface IObserver<TObservable> : IDisposable where TObservable : INotifyPropertyChanged
{
    ISubscription<TObservable, TValue> Subscribe<TValue>(string propertyName, Func<TObservable, TValue> valueFunc);
    void Backfill();
}
