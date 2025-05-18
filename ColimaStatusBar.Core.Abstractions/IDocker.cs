using Swallow.Flux;

namespace ColimaStatusBar.Core.Abstractions;

public enum ContainerState { Created, Running, Paused, Stopped, Restarting, Exited, Removing, Dead }

public sealed record RunningContainer(string Id, string Name, string Image, string Context, ContainerState State)
{
    public bool CanStart => State is ContainerState.Created or ContainerState.Stopped or ContainerState.Exited;
    
    public bool CanStop => State is ContainerState.Running or ContainerState.Restarting;
    
    public bool CanRemove => State is ContainerState.Created or ContainerState.Stopped or ContainerState.Exited;
}

public sealed record ContainerStatusChanged(string Id) : INotification;
public sealed record ContainerAdded(string Id) : INotification;
public sealed record ContainerRemoved(string Id) : INotification;

public interface IDocker : IStore
{
    IEnumerable<RunningContainer> RunningContainers { get; }

    RunningContainer? GetContainer(string id);
}
