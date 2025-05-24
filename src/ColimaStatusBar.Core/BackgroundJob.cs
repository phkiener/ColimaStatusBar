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
            pollTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            
            // Don't DisposeAsync here, we don't need to wait for the spawned tasks - just make sure that no new ones are getting spawned
            pollTimer.Dispose();
            pollTimer = null;
        }
    }

    private async void OnTick(object? state)
    {
        try
        {
            var cancellationToken = state is CancellationTokenSource tokenSource ? tokenSource.Token : CancellationToken.None;
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Run(cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException or TaskCanceledException)
        {
            OnException(e);
        }
    }
    
    protected abstract TimeSpan Interval { get; }
    
    protected abstract Task Run(CancellationToken cancellationToken);

    protected virtual void OnException(Exception exception)
    {
        // Do nothing by default.
    }
}
