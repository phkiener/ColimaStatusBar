using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.Core;

public static class Commands
{
    public sealed record StartColima : ICommand;
    public sealed record StopColima : ICommand;

    public sealed record LaunchAtLogin(bool Enabled) : ICommand;
    
    public sealed record Initialize : ICommand;
    public sealed record Shutdown : ICommand;
}
