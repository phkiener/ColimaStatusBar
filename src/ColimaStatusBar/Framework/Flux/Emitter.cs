namespace ColimaStatusBar.Framework.Flux;

public interface INotification;

public sealed class Emitter
{
    public void Emit(INotification notification)
    {
        Console.WriteLine($"Emitting {notification.GetType().Name}");
        OnEmit?.Invoke(this, notification);
    }
    
    public void Emit<T>() where T : INotification, new() => Emit(new T());

    public event EventHandler<INotification>? OnEmit;
}
