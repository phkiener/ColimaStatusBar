using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Core.Platform;
using Swallow.Flux;

namespace ColimaStatusBar.Core.Docker;

public sealed class DockerStore : AbstractStore, IDocker
{
    private readonly IShellExecutor shellExecutor;
    private readonly DockerPollingJob pollingJob;
    private readonly Dictionary<string, RunningContainer> containers = [];
    private readonly HashSet<string> skipPolling = new();
    
    public DockerStore(IShellExecutor shellExecutor, IEmitter emitter) : base(emitter)
    {
        this.shellExecutor = shellExecutor;
        pollingJob = new DockerPollingJob(shellExecutor, ContainersPolled);
        
        Register<Commands.Initialize>(StartPolling);
        Register<Commands.Shutdown>(StopPolling);
        Register<Commands.StartContainer>(StartContainer);
        Register<Commands.StopContainer>(StopContainer);
        Register<Commands.RemoveContainer>(RemoveContainer);
    }

    public IEnumerable<RunningContainer> RunningContainers => containers.Values;
    
    public RunningContainer? GetContainer(string id) => containers.GetValueOrDefault(id);

    private void StartPolling()
    {
        pollingJob.Start();
    }

    private async Task StopPolling()
    {
        await pollingJob.Stop();
    }

    private async Task StartContainer(Commands.StartContainer command, CancellationToken cancellationToken)
    {
        var container = containers.GetValueOrDefault(command.Id);
        if (container is not null)
        {
            using (SkipPollingFor(container.Id))
            {
                containers[container.Id] = container with { State = ContainerState.Running };
                Emit(new ContainerStatusChanged(command.Id));

                await shellExecutor.Run("docker", ["--context", container.Context, "start", container.Id], cancellationToken);
            }
        }
    }

    private async Task StopContainer(Commands.StopContainer command, CancellationToken cancellationToken)
    {
        var container = containers.GetValueOrDefault(command.Id);
        if (container is not null)
        {
            using (SkipPollingFor(container.Id))
            {
                containers[container.Id] = container with { State = ContainerState.Stopped };
                Emit(new ContainerStatusChanged(command.Id));

                await shellExecutor.Run("docker", ["--context", container.Context, "stop", container.Id], cancellationToken);
            }
        }
    }
    private async Task RemoveContainer(Commands.RemoveContainer command, CancellationToken cancellationToken)
    {
        var container = containers.GetValueOrDefault(command.Id);
        if (container is not null)
        {
            using (SkipPollingFor(container.Id))
            {
                containers.Remove(container.Id);
                Emit(new ContainerRemoved(command.Id));
                
                await shellExecutor.Run("docker", ["--context", container.Context, "rm", container.Id], cancellationToken);
            }
        }
    }
    
    private void ContainersPolled(RunningContainer[] polledContainers)
    {
        var addedContainers = polledContainers.ExceptBy(containers.Keys, static p => p.Id).ToList();
        var removedContainers = containers.Values.ExceptBy(polledContainers.Select(static p => p.Id), static p => p.Id).ToList();

        foreach (var addedContainer in addedContainers)
        {
            containers.Add(addedContainer.Id, addedContainer);
            Emit(new ContainerAdded(addedContainer.Id));
        }
        
        foreach (var removedContainer in removedContainers)
        {
            containers.Remove(removedContainer.Id);
            Emit(new ContainerRemoved(removedContainer.Id));
        }
        
        foreach (var container in polledContainers.Except(addedContainers))
        {
            var matchingContainer = containers.GetValueOrDefault(container.Id);
            
            // The image shouldn't change, only the status - we care only for that one
            if (matchingContainer is not null && matchingContainer.State != container.State && !skipPolling.Contains(container.Id))
            {
                containers[container.Id] = matchingContainer with { State = container.State };
                Emit(new ContainerStatusChanged(container.Id));
            }
        }
    }

    private IDisposable SkipPollingFor(string profileName)
    {
        skipPolling.Add(profileName);
        return new ExecuteOnDispose(() => skipPolling.Remove(profileName));
    }
}
