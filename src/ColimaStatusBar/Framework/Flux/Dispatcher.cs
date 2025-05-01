namespace ColimaStatusBar.Framework.Flux;

public sealed class Dispatcher(IEnumerable<IStore> stores)
{
    public async Task Invoke(ICommand command)
    {
        Console.WriteLine($"Dispatching {command.GetType().Name}");
        foreach (var store in stores)
        {
            try
            {
                await store.Handle(command);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync($"Error in store {store.GetType().Name}: {e}");
            }
        }
    }
    
    public Task Invoke<T>() where T : ICommand, new() => Invoke(new T());
}
