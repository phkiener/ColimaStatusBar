using Swallow.Flux;

namespace ColimaStatusBar.Core.Abstractions;

public static class Commands
{
    public sealed record StartProfile(string Name) : ICommand;
    public sealed record StopProfile(string Name) : ICommand;
    
    public sealed record StartContainer(string Id) : ICommand;
    public sealed record StopContainer(string Id) : ICommand;
    public sealed record RemoveContainer(string Id) : ICommand;
    
    public sealed record LaunchAtLogin(bool Enabled) : ICommand;
    
    public sealed record Initialize : ICommand;
    public sealed record Shutdown : ICommand;
}
