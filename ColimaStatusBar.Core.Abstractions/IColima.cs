using Swallow.Flux;

namespace ColimaStatusBar.Core.Abstractions;

public enum ProfileStatus
{
    Stopped,
    Stopping,
    Starting,
    Running
}

public sealed record ColimaProfileInfo(string Name, ProfileStatus Status, int CpuCount, long MemoryBytes, long DiskBytes);

public sealed record ProfileStatusChanged(string Name) : INotification;
public sealed record ProfileAdded(string Name) : INotification;
public sealed record ProfileRemoved(string Name) : INotification;

public interface IColima : IStore
{
    ProfileStatus OverallStatus { get; }

    IEnumerable<ColimaProfileInfo> Profiles { get; }

    ColimaProfileInfo? GetProfile(string name);
}
