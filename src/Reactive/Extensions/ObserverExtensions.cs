using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Reactive.Extensions;

public static class ObserverExtensions
{
    public static ISubscription<TObservable, TValue> Subscribe<TObservable, TValue>(
        this IObserver<TObservable> self,
        Expression<Func<TObservable, TValue>> property)
        where TObservable : INotifyPropertyChanged
    {
        if (property.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("The given expression is not a property expression.", nameof(property));
        }

        return self.Subscribe(propertyInfo.Name, v => (TValue)propertyInfo.GetValue(v)!);
    }
}
