using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Core.Platform;
using Swallow.Flux;

namespace ColimaStatusBar.Core.Colima;

public sealed class ColimaStore : AbstractStore, IColima
{
    private readonly IShellExecutor shellExecutor;
    private readonly ColimaPollingJob pollingJob;
    private readonly Dictionary<string, ColimaProfileInfo> profiles = [];
    private readonly HashSet<string> skipPolling = new();
    
    public ColimaStore(IShellExecutor shellExecutor, IEmitter emitter) : base(emitter)
    {
        this.shellExecutor = shellExecutor;
        pollingJob = new ColimaPollingJob(shellExecutor, ProfilesPolled);

        Register<Commands.Initialize>(StartPolling);
        Register<Commands.Shutdown>(StopPolling);
        Register<Commands.StartProfile>(StartProfile);
        Register<Commands.StopProfile>(StopProfile);
    }

    public ProfileStatus OverallStatus => profiles.Values.Any(static p => p.Status is ProfileStatus.Running) ? ProfileStatus.Running : ProfileStatus.Stopped;
    public IEnumerable<ColimaProfileInfo> Profiles => profiles.Values;
    
    public ColimaProfileInfo? GetProfile(string name) => profiles.GetValueOrDefault(name);

    private void StartPolling()
    {
        pollingJob.Start();
    }

    private async Task StopPolling()
    {
        await pollingJob.Stop();
    }
    
    private async Task StartProfile(Commands.StartProfile command, CancellationToken cancellationToken)
    {
        var profile = GetProfile(command.Name);
        if (profile is not null)
        {
            using (SkipPollingFor(profile.Name))
            {
                profiles[profile.Name] = profile with { Status = ProfileStatus.Starting };
                Emit(new ProfileStatusChanged(command.Name));

                await shellExecutor.Run("colima", ["start", "-p", command.Name], cancellationToken);
                Emit(new ProfileStatusChanged(command.Name));
                
                profiles[profile.Name] = profile with { Status = ProfileStatus.Running };
                Emit(new ProfileStatusChanged(command.Name));
            }
        }
    }
    
    private async Task StopProfile(Commands.StopProfile command, CancellationToken cancellationToken)
    {
        var profile = GetProfile(command.Name);
        if (profile is not null)
        {
            using (SkipPollingFor(profile.Name))
            {
                profiles[profile.Name] = profile with { Status = ProfileStatus.Stopping };
                Emit(new ProfileStatusChanged(command.Name));

                await shellExecutor.Run("colima", ["stop", "-p", command.Name], cancellationToken);
                Emit(new ProfileStatusChanged(command.Name));
                
                profiles[profile.Name] = profile with { Status = ProfileStatus.Stopped };
                Emit(new ProfileStatusChanged(command.Name));
            }
        }
    }

    private void ProfilesPolled(ColimaProfileInfo[] polledProfiles)
    {
        var addedProfiles = polledProfiles.ExceptBy(profiles.Keys, static p => p.Name).ToList();
        var removedProfiles = profiles.Values.ExceptBy(polledProfiles.Select(static p => p.Name), static p => p.Name).ToList();

        foreach (var addedProfile in addedProfiles)
        {
            profiles.Add(addedProfile.Name, addedProfile);
            Emit(new ProfileAdded(addedProfile.Name));
        }
        
        foreach (var removedProfile in removedProfiles)
        {
            profiles.Remove(removedProfile.Name);
            Emit(new ProfileRemoved(removedProfile.Name));
        }
        
        foreach (var profile in polledProfiles.Except(addedProfiles))
        {
            var matchingProfile = GetProfile(profile.Name);
            
            // CPU, Memory and Disk shouldn't change - we just ignore that part
            if (matchingProfile is not null && matchingProfile.Status != profile.Status && !skipPolling.Contains(profile.Name))
            {
                profiles[profile.Name] = matchingProfile with { Status = profile.Status };
                Emit(new ProfileStatusChanged(profile.Name));
            }
        }
    }

    private IDisposable SkipPollingFor(string profileName)
    {
        skipPolling.Add(profileName);
        return new ExecuteOnDispose(() => skipPolling.Remove(profileName));
    }
}
