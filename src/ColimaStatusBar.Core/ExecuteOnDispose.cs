namespace ColimaStatusBar.Core;

internal sealed class ExecuteOnDispose(Action onDispose) : IDisposable
{
    public void Dispose()
    {
        onDispose.Invoke();
    }
}
