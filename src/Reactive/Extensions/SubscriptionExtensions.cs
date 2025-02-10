using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Reactive.Extensions;

public static class SubscriptionExtensions
{
    public static ISubscription<TObservable, TValue> Bind<TObservable, TValue, TTarget>(
        this ISubscription<TObservable, TValue> self,
        TTarget target,
        Expression<Func<TTarget, TValue>> property)
        where TObservable : INotifyPropertyChanged
    {
        if (property.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("The given expression is not a property expression.", nameof(property));
        }
        
        return self.Handle(value => propertyInfo.SetValue(target, value));
    }
    
    public static ISubscription<TObservable, TValue> Bind<TObservable, TValue, TTarget, TResult>(
        this ISubscription<TObservable, TValue> self,
        TTarget target,
        Expression<Func<TTarget, TResult>> property,
        Func<TValue, TResult> conversion)
        where TObservable : INotifyPropertyChanged
    {
        if (property.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("The given expression is not a property expression.", nameof(property));
        }
        
        return self.Handle(value => propertyInfo.SetValue(target, conversion(value)));
    }
}
