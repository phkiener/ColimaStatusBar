using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.Core;

public enum ContainerState { Created, Running, Paused, Stopped, Restarting, Exited, Removing, Dead }

public sealed record RunningContainer(string Id, string Name, string Image, ContainerState State);

public sealed record RunningContainersChanged : INotification;

public sealed class RunningContainersStore : IStore, IAsyncDisposable
{
    private readonly Emitter emitter;
    private readonly List<RunningContainer> runningContainers = [];
    
    private readonly CancellationTokenSource pollingCancelled = new();
    private Task pollingTask = Task.CompletedTask;
    private bool isPolling => !pollingCancelled.IsCancellationRequested;
    private string? currentSocket;
    
    public IReadOnlyList<RunningContainer> RunningContainers => runningContainers.AsReadOnly();

    public RunningContainersStore(Emitter emitter)
    {
        this.emitter = emitter;
        this.emitter.OnEmit += ObserveSocketChange;
    }

    private void ObserveSocketChange(object? sender, INotification notification)
    {
        if (notification is SocketChanged socketChanged)
        {
            currentSocket = socketChanged.SocketAddress;
        }
    }

    Task IStore.Handle(ICommand command)
    {
        if (command is Commands.Initialize)
        {
            pollingTask = FetchRunningContainersAsync();
            return Task.CompletedTask;
        }

        if (command is Commands.Shutdown)
        {
            _ = pollingCancelled.CancelAsync();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private async Task FetchRunningContainersAsync()
    {
        await Task.Yield(); // force a yield, the rest should happen on a background thread

        var pollTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (isPolling)
        {
            try
            {
                if (currentSocket is null)
                {
                    if (RunningContainers is not [])
                    {
                        runningContainers.Clear();
                        emitter.Emit<RunningContainersChanged>();
                    }
                }
                else
                {
                    var response = await Infrastructure.Docker.StatusAsync(currentSocket, pollingCancelled.Token);
                    bool containersChanged = false;

                    foreach (var container in response)
                    {
                        var existingContainer = runningContainers.FirstOrDefault(c => c.Id == container.Id);
                        if (existingContainer is not null && existingContainer != container)
                        {
                            var index = runningContainers.IndexOf(existingContainer);
                            runningContainers[index] = container;
                            
                            containersChanged = true;
                        }

                        if (existingContainer is null)
                        {
                            runningContainers.Add(container);
                            containersChanged = true;
                        }
                    }

                    var removedContainers = runningContainers.RemoveAll(c => response.All(r => c.Id != r.Id));
                    if (containersChanged || removedContainers > 0)
                    {
                        emitter.Emit<RunningContainersChanged>();
                    }
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
        emitter.OnEmit -= ObserveSocketChange;

        if (isPolling)
        {
            await pollingCancelled.CancelAsync();
            await pollingTask;

            pollingCancelled.Dispose();
        }
    }
}
