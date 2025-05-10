namespace ColimaStatusBar.Framework.Flux;

public interface ITargetedBinding<out T> where T : class
{
    ITargetedBinding<T> To<TNotification>(Action<T> reaction) where TNotification : INotification;
}

public sealed class Binder(Emitter emitter) : IDisposable
{
    private readonly List<IDisposable> bindings = [];
    
    public ITargetedBinding<T> Bind<T>(T target) where T : class
    {
        var binding = new Binding<T>(target, null, emitter);
        bindings.Add(binding);

        return binding;
    }
    
    public ITargetedBinding<T> Bind<T>(T target, Action<Action> wrapper) where T : class
    {
        var binding = new Binding<T>(target, wrapper, emitter);
        bindings.Add(binding);

        return binding;
    }

    private sealed class Binding<T> : ITargetedBinding<T>, IDisposable where T : class
    {
        private readonly T target;
        private readonly Action<Action>? wrapper;
        private readonly Emitter emitter;
        private readonly List<(Type Type, Action<T> Reaction)> subscriptions = [];

        public Binding(T target, Action<Action>? wrapper, Emitter emitter)
        {
            this.target = target;
            this.wrapper = wrapper;
            this.emitter = emitter;

            emitter.OnEmit += InvokeSubscribers;
        }

        public ITargetedBinding<T> To<TNotification>(Action<T> reaction) where TNotification : INotification
        {
            subscriptions.Add((typeof(TNotification), reaction));
            Invoke(reaction);
            
            return this;
        }

        public void Dispose()
        {
            emitter.OnEmit -= InvokeSubscribers;
        }

        private void InvokeSubscribers(object? sender, INotification e)
        {
            foreach (var subscription in subscriptions.Where(t => t.Type == e.GetType()))
            {
                Invoke(subscription.Reaction);
            }
        }

        private void Invoke(Action<T> reaction)
        {
            if (wrapper is not null)
            {
                wrapper.Invoke(() => reaction.Invoke(target));
            }
            else
            {
                reaction.Invoke(target);
            }
        }
    }

    public void Dispose()
    {
        foreach (var binding in bindings)
        {
            binding.Dispose();
        }
    }
}
