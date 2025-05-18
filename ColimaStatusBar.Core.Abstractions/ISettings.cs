using Swallow.Flux;

namespace ColimaStatusBar.Core.Abstractions;

public sealed record LaunchAtLoginChanged(bool launchAtLogin) : INotification;

public interface ISettings : IStore
{
    bool LaunchAtLogin { get; }
}
