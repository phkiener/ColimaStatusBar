namespace ColimaStatusBar.Core;

public abstract class BackgroundJob
{
    private CancellationTokenSource? cancellationTokenSource;
    private Timer? pollTimer;
    
    public void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        pollTimer = new Timer(OnTick, cancellationTokenSource, TimeSpan.Zero, Interval);
    }

    public async Task Stop()
    {
        if (cancellationTokenSource is not null)
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource = null;
        }

        if (pollTimer is not null)
        {
            await pollTimer.DisposeAsync();
            pollTimer = null;
        }
    }

    private async void OnTick(object? state)
    {
        try
        {
            var cancellationToken = state is CancellationTokenSource tokenSource ? tokenSource.Token : CancellationToken.None;
            await Run(cancellationToken);
        }
        catch (Exception e)
        {
            OnException(e);
        }
    }
    
    protected abstract TimeSpan Interval { get; }
    
    protected abstract Task Run(CancellationToken token);

    protected virtual void OnException(Exception exception)
    {
        // Do nothing by default.
    }
}
