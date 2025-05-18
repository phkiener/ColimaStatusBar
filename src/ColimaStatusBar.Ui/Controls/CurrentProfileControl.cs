using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Ui.Framework;
using Swallow.Flux;

namespace ColimaStatusBar.Ui.Controls;

public sealed class CurrentProfileControl(IColima colima, IDispatcher dispatcher, IBinder binder) : Control<NSMenu>(dispatcher, binder)
{
    private sealed record ProfileItems(string Name, NSMenuItem Status, NSMenuItem Details, NSMenuItem Manage) : IDisposable
    {
        public void Dispose()
        {
            Status.Dispose();
            Details.Dispose();
            Manage.Dispose();
        }
    }

    private const long gibibytesFactor = 1073741824;

    private readonly List<ProfileItems> profileItems = new();
    private readonly NSMenuItem manageDefaultProfile = new() { Hidden = true };
    private readonly NSMenuItem manageProfiles = new("Manage profiles") { Hidden = true, Submenu = new NSMenu() };

    protected override void OnAttach(NSMenu target)
    {
        target.AddItem(manageDefaultProfile);
        target.AddItem(manageProfiles);
        
        Binder.BindControl(target)
            .To<ProfileAdded>(OnProfileAdded)
            .To<ProfileRemoved>(OnProfileRemoved)
            .To<ProfileStatusChanged>(OnProfileStatusChanged);
        
        manageDefaultProfile.Activated += (_, _) => ToggleProfile(null);
        foreach (var profile in colima.Profiles)
        {
            AddProfile(target, profile);
        }

        UpdateDefaultProfile();
    }

    private void OnProfileAdded(NSMenu menu, ProfileAdded notification)
    {
        var profile = colima.GetProfile(notification.Name);
        if (profile is not null && profileItems.All(p => p.Name != notification.Name))
        {
            AddProfile(menu, profile);
            UpdateDefaultProfile();
        }
    }

    private void OnProfileRemoved(NSMenu menu, ProfileRemoved notification)
    {
        var profileItem = profileItems.FirstOrDefault(x => x.Name == notification.Name);
        if (profileItem is null)
        {
            return;
        }
        
        menu.RemoveItem(profileItem.Details);
        menu.RemoveItem(profileItem.Status);
        manageProfiles.Submenu?.RemoveItem(profileItem.Manage);
        
        profileItems.Remove(profileItem);
        profileItem.Dispose();
        
        UpdateDefaultProfile();
    }

    private void OnProfileStatusChanged(NSMenu menu, ProfileStatusChanged notification)
    {
        var profileDetails = profileItems.FirstOrDefault(x => x.Name == notification.Name);
        var profile = colima.GetProfile(notification.Name);

        if (profileDetails is not null && profile is not null)
        {
            UpdateProfile(profileDetails, profile);
            UpdateDefaultProfile();
        }
    }

    private void AddProfile(NSMenu menu, ColimaProfileInfo profile)
    {
        var profileDetails = new ProfileItems(
            Name: profile.Name,
            Status: new NSMenuItem { Enabled = false },
            Details: new NSMenuItem { Enabled = false },
            Manage: new NSMenuItem());
        
        profileDetails.Manage.Activated += (_, _) => ToggleProfile(profile.Name);
        profileItems.Add(profileDetails);
        UpdateProfile(profileDetails, profile);
        
        var position = menu.IndexOf(manageDefaultProfile);
        menu.InsertItem(profileDetails.Details, position);
        menu.InsertItem(profileDetails.Status, position);
        
        manageProfiles.Submenu?.AddItem(profileDetails.Manage);
    }

    private async void ToggleProfile(string? profileName)
    {
        var profile = profileName is null
            ? colima.Profiles.SingleOrDefault()
            : colima.GetProfile(profileName);
        
        if (profile?.Status is ProfileStatus.Running)
        {
            await Dispatcher.Dispatch(new Commands.StopProfile(profile.Name));
        }
        else if (profile?.Status is ProfileStatus.Stopped)
        {
            await Dispatcher.Dispatch(new Commands.StartProfile(profile.Name));
        }
    }

    private static void UpdateProfile(ProfileItems items, ColimaProfileInfo profile)
    {
        items.Status.Hidden = profile.Status is not ProfileStatus.Running;
        items.Status.Title = profile.Status switch
        {
            ProfileStatus.Running => $"Running profile '{profile.Name}'",
            _ => $"Profile '{profile.Name}' is stopped"
        };
        
        items.Details.Hidden = profile.Status is not ProfileStatus.Running;
        items.Details.Title = string.Join(" | ",
            $"{profile.CpuCount} CPU",
            $"{AsGibibytes(profile.MemoryBytes)} RAM",
            $"{AsGibibytes(profile.DiskBytes)} Disk");

        items.Manage.Enabled = profile.Status is ProfileStatus.Running or ProfileStatus.Stopped;
        items.Manage.Title = profile.Status switch
        {
            ProfileStatus.Stopping => $"Profile '{profile.Name}' is stopping...",
            ProfileStatus.Starting => $"Profile '{profile.Name}' is starting...",
            ProfileStatus.Running => $"Stop profile '{profile.Name}'",
            _ => $"Start profile '{profile.Name}'"
        };
    }

    private void UpdateDefaultProfile()
    {
        var singleProfile = colima.Profiles.Count() is 0 or 1;
        
        manageDefaultProfile.Hidden = !singleProfile;
        manageDefaultProfile.Title = colima.OverallStatus switch
        {
            ProfileStatus.Stopping => "Colima is stopping...",
            ProfileStatus.Starting => "Colima is starting...",
            ProfileStatus.Running => "Stop colima",
            _ => "Start colima"
        };
        
        manageProfiles.Hidden = singleProfile;
    }

    private static string AsGibibytes(long value) => $"{value / (double)gibibytesFactor} GiB";
}
