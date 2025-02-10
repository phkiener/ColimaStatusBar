using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Reactive;

public sealed class Observer<T> : IDisposable where T : INotifyPropertyChanged
{
    private readonly List<Subscription> subscriptions = [];
    private readonly T observable;
    
    public Observer(T observable)
    {
        this.observable = observable;
        this.observable.PropertyChanged += OnPropertyChanged;
    }

    public SubscriptionConfiguration<T, TValue> Subscribe<TValue>(Expression<Func<T, TValue>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("Can only subscribe to a property.");
        }

        var subscription = new Subscription<TValue>(propertyInfo.Name, propertyInfo.GetValue);
        subscriptions.Add(subscription);
        
        return new SubscriptionConfiguration<T, TValue>(subscription);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var matchingSubscriptions = subscriptions.Where(s => s.Matches(e)).ToList();
        
        foreach (var subscription in matchingSubscriptions)
        {
            subscription.Invoke(observable);
        }
    }

    public void Backfill()
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Invoke(observable);
        }
    }

    public void Dispose()
    {
        observable.PropertyChanged -= OnPropertyChanged;
    }

    internal abstract class Subscription(string propertyName, Func<object?, object?> valueAccessor)
    {
        public void Invoke(T sender)
        {
            var value = valueAccessor(sender);
            Handle(value);
        }

        public bool Matches(PropertyChangedEventArgs eventArgs)
        {
            return eventArgs.PropertyName == propertyName;
        }

        protected abstract void Handle(object? value);
    }

    internal sealed class Subscription<TValue>(string propertyName, Func<object?, object?> valueAccessor) : Subscription(propertyName, valueAccessor)
    {
        private readonly List<Action<TValue>> handlers = [];
        private Action<Action>? decorator;

        public void AddHandler(Action<TValue> handler)
        {
            handlers.Add(handler);
        }

        public void AddDecorator(Action<Action> func)
        {
            decorator = func;
        }

        protected override void Handle(object? value)
        {
            var typedValue = (TValue)value!;
            foreach (var handler in handlers)
            {
                if (decorator is not null)
                {
                    decorator.Invoke(() => handler.Invoke(typedValue));
                }
                else
                {
                    handler.Invoke(typedValue);
                }
            }
        }
    }
}

public sealed class SubscriptionConfiguration<TSender, TValue> where TSender : INotifyPropertyChanged
{
    private readonly Observer<TSender>.Subscription<TValue> subscription;
    
    internal SubscriptionConfiguration(Observer<TSender>.Subscription<TValue> subscription)
    {
        this.subscription = subscription;
    }

    public SubscriptionConfiguration<TSender, TValue> Decorate(Action<Action> decorator)
    {
        subscription.AddDecorator(decorator);
        return this;
    }

    public SubscriptionConfiguration<TSender, TValue> Handle(Action<TValue> handler)
    {
        subscription.AddHandler(handler);
        return this;
    }

    public SubscriptionConfiguration<TSender, TValue> Bind<TTarget, TConvert>(TTarget target, Expression<Func<TTarget, TConvert>> propertyExpression, Func<TValue, TConvert> converter)
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("Can only subscribe to a property.");
        }

        return Handle(update => { propertyInfo.SetValue(target, converter(update)); });
    }

    public SubscriptionConfiguration<TSender, TValue> Bind<TTarget>(TTarget target, Expression<Func<TTarget, TValue>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("Can only subscribe to a property.");
        }

        return Handle(update => propertyInfo.SetValue(target, update));
    }
}
