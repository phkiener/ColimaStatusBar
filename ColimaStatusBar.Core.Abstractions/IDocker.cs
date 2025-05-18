namespace ColimaStatusBar.Core.Abstractions;

public enum ContainerState { Created, Running, Paused, Stopped, Restarting, Exited, Removing, Dead }

public sealed record RunningContainer(string Id, string Name, string Image, ContainerState State)
{
    public bool CanStart => State is ContainerState.Created or ContainerState.Stopped or ContainerState.Exited;
    
    public bool CanStop => State is ContainerState.Running or ContainerState.Restarting;
    
    public bool CanRemove => State is ContainerState.Created or ContainerState.Stopped or ContainerState.Exited;
}
