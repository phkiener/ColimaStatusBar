using System.ComponentModel;

namespace Reactive.Implementation;

internal sealed class DefaultObserver<TObservable> : IObserver<TObservable> where TObservable : INotifyPropertyChanged
{
    private readonly Dictionary<string, List<ChangeHandler<TObservable>>> changeHandlers = new();
    private readonly TObservable observable;
    
    public DefaultObserver(TObservable observable)
    {
        this.observable = observable;
        observable.PropertyChanged += OnPropertyChanged;
    }

    public ISubscription<TObservable, TValue> Subscribe<TValue>(string propertyName, Func<TObservable, TValue> valueFunc)
    {
        var changeHandler = new ChangeHandler<TObservable>(v => valueFunc(v));
        if (changeHandlers.TryGetValue(propertyName, out var handlers))
        {
            handlers.Add(changeHandler);
        }
        else
        {
            changeHandlers[propertyName] = [changeHandler];
        }

        return new ChangeHandler<TObservable>.Configurator<TValue>(changeHandler);
    }

    public void Backfill()
    {
        foreach (var handler in changeHandlers.SelectMany(static h => h.Value))
        {
            handler.Handle(observable);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!ReferenceEquals(sender, observable)
            || e.PropertyName is null
            || !changeHandlers.TryGetValue(e.PropertyName, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            handler.Handle(observable);
        }
    }
    
    public void Dispose()
    {
        observable.PropertyChanged -= OnPropertyChanged;
    }
}
