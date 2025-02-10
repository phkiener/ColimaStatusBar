using System.ComponentModel;
using Reactive.Implementation;

namespace Reactive;

public static class Observer
{
    public static IObserver<T> Create<T>(T value) where T : INotifyPropertyChanged => new DefaultObserver<T>(value);
}
