using ColimaStatusBar.Framework;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.Core;

public enum ColimaStatus { Stopped, Starting, Running, Stopping }

public sealed record ColimaStatusChanged : INotification;

public sealed class ColimaStatusStore(Emitter emitter) : IStore, IDisposable, IAsyncDisposable
{
    private readonly CancellationTokenSource pollingCancelled = new();
    private Task pollingTask = Task.CompletedTask;
    private bool isPolling => !pollingCancelled.IsCancellationRequested;
    
    public ColimaStatus CurrentStatus { get; private set; } = ColimaStatus.Stopped;
    
    Task IStore.Handle(ICommand command)
    {
        if (command is Commands.Initialize)
        {
            pollingTask = FetchColimaStatusAsync();
            return Task.CompletedTask;
        }

        if (command is Commands.Shutdown)
        {
            pollingCancelled.Cancel();
            return Task.CompletedTask;
        }

        if (command is Commands.StartColima && CurrentStatus is not (ColimaStatus.Running or ColimaStatus.Starting))
        {
            CurrentStatus = ColimaStatus.Starting;
            emitter.Emit<ColimaStatusChanged>();

            _ = ProcessRunner.RunProcessAsync("/opt/homebrew/bin/colima", ["start"], pollingCancelled.Token);
            return Task.CompletedTask;
        }

        if (command is Commands.StopColima && CurrentStatus is not (ColimaStatus.Stopped or ColimaStatus.Stopping))
        {
            CurrentStatus = ColimaStatus.Stopping;
            emitter.Emit<ColimaStatusChanged>();

            _ = ProcessRunner.RunProcessAsync("/opt/homebrew/bin/colima", ["stop"], pollingCancelled.Token);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private async Task FetchColimaStatusAsync()
    {
        await Task.Yield(); // force a yield, the rest should happen on a background thread

        var pollTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (isPolling)
        {
            try
            {
                var(exitCode, output) = await ProcessRunner.RunProcessAsync("/opt/homebrew/bin/colima", ["status", "--json"], pollingCancelled.Token);
                var fetchedStatus = exitCode is 1 ? ColimaStatus.Stopped : ColimaStatus.Running;

                if (CurrentStatus != fetchedStatus)
                {
                    CurrentStatus = fetchedStatus;
                    emitter.Emit<ColimaStatusChanged>();
                }

                await pollTimer.WaitForNextTickAsync(pollingCancelled.Token);
            }
            catch
            {
                // ignore
            }
        }
    }

    public void Dispose()
    {
        pollingCancelled.Cancel();
        pollingCancelled.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await pollingCancelled.CancelAsync();
        await pollingTask;

        pollingCancelled.Dispose();
    }
}
