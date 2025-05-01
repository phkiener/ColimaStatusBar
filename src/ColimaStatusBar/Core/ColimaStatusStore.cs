using ColimaStatusBar.Core.Infrastructure;
using ColimaStatusBar.Framework;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.Core;

public enum ColimaStatus { Stopped, Starting, Running, Stopping }

public sealed record RunningProfile(string Name, string SocketAddress, int CpuCores, long MemoryBytes, long DiskBytes);

public sealed record ColimaStatusChanged : INotification;
public sealed record ColimaProfileChanged : INotification;

internal sealed record SocketChanged(string? SocketAddress) : INotification;

public sealed class ColimaStatusStore(Emitter emitter) : IStore, IAsyncDisposable
{
    private readonly CancellationTokenSource pollingCancelled = new();
    private Task pollingTask = Task.CompletedTask;
    private bool isPolling => !pollingCancelled.IsCancellationRequested;
    
    public ColimaStatus CurrentStatus { get; private set; } = ColimaStatus.Stopped;
    public RunningProfile? CurrentProfile { get; private set; }

    async Task IStore.Handle(ICommand command)
    {
        if (command is Commands.Initialize)
        {
            pollingTask = FetchColimaStatusAsync();
            return;
        }

        if (command is Commands.Shutdown)
        {
            await pollingCancelled.CancelAsync();
            return;
        }

        if (command is Commands.StartColima && CurrentStatus is not (ColimaStatus.Running or ColimaStatus.Starting))
        {
            CurrentStatus = ColimaStatus.Starting;
            emitter.Emit<ColimaStatusChanged>();

            _ = ProcessRunner.RunProcessAsync("/opt/homebrew/bin/colima", ["start"], pollingCancelled.Token);
            return;
        }

        if (command is Commands.StopColima && CurrentStatus is not (ColimaStatus.Stopped or ColimaStatus.Stopping))
        {
            CurrentStatus = ColimaStatus.Stopping;
            emitter.Emit<ColimaStatusChanged>();

            _ = ProcessRunner.RunProcessAsync("/opt/homebrew/bin/colima", ["stop"], pollingCancelled.Token);
            return;
        }
    }

    private async Task FetchColimaStatusAsync()
    {
        await Task.Yield(); // force a yield, the rest should happen on a background thread

        var pollTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (isPolling)
        {
            try
            {
                var runningProfile = await Colima.StatusAsync(pollingCancelled.Token);
                var fetchedStatus = runningProfile is null ? ColimaStatus.Stopped : ColimaStatus.Running;

                if (CurrentStatus != fetchedStatus)
                {
                    CurrentStatus = fetchedStatus;
                    emitter.Emit<ColimaStatusChanged>();
                }

                if (CurrentProfile != runningProfile)
                {
                    CurrentProfile = runningProfile;
                    emitter.Emit<ColimaProfileChanged>();

                    emitter.Emit(new SocketChanged(CurrentProfile?.SocketAddress));
                }

                await pollTimer.WaitForNextTickAsync(pollingCancelled.Token);
            }
            catch
            {
                // ignore
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (isPolling)
        {
            await pollingCancelled.CancelAsync();
            await pollingTask;

            pollingCancelled.Dispose();
        }
    }
}
