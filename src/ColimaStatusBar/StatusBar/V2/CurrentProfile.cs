using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar.V2;

public sealed class CurrentProfile(ColimaStatusStore store, Dispatcher dispatcher, Binder binder) : Control<NSMenu>(dispatcher, binder)
{
    private const long gibibytesFactor = 1073741824;

    private readonly NSMenuItem currentProfileItem = new() { Enabled = false };
    private readonly NSMenuItem profileDetailsItem = new() { Enabled = false };
    private readonly NSMenuItem toggleColimaItem = new();

    protected override void OnAttach(NSMenu target)
    {
        target.AddItem(currentProfileItem);
        target.AddItem(profileDetailsItem);
        target.AddItem(toggleColimaItem);

        Binder.BindControl(currentProfileItem).To<ColimaProfileChanged>(UpdateRunningProfile);
        Binder.BindControl(profileDetailsItem).To<ColimaProfileChanged>(UpdateProfileDetails);
        Binder.BindControl(toggleColimaItem).To<ColimaStatusChanged>(UpdateStatusToggle);

        toggleColimaItem.Activated += OnStatusToggleClicked;
    }

    private void UpdateRunningProfile(NSMenuItem target)
    {
        target.Title = store.CurrentProfile is null
            ? "Colima is not running"
            : $"Running profile '{store.CurrentProfile.Name}'";
    }

    private void UpdateProfileDetails(NSMenuItem target)
    {
        if (store.CurrentProfile is null)
        {
            target.Hidden = true;
            return;
        }

        target.Title = string.Join(" | ",
            $"{store.CurrentProfile.CpuCores} CPU",
            $"{AsGibibytes(store.CurrentProfile.MemoryBytes)} RAM",
            $"{AsGibibytes(store.CurrentProfile.DiskBytes)} Disk");

        target.Hidden = false;
    }

    private void UpdateStatusToggle(NSMenuItem target)
    {
        target.Title = store.CurrentStatus switch
        {
            ColimaStatus.Stopped => "Start colima",
            ColimaStatus.Starting => "Colima is starting...",
            ColimaStatus.Running => "Stop colima",
            ColimaStatus.Stopping => "Colima is stopping...",
            _ => throw new ArgumentOutOfRangeException(nameof(target), store.CurrentStatus, null)
        };
            
        target.Enabled = store.CurrentStatus is ColimaStatus.Running or ColimaStatus.Stopped;
    }

    private async void OnStatusToggleClicked(object? sender, EventArgs args)
    {
        if (store.CurrentStatus is ColimaStatus.Stopped)
        {
            await Dispatcher.Invoke<Commands.StartColima>();
        }

        if (store.CurrentStatus is ColimaStatus.Running)
        {
            await Dispatcher.Invoke<Commands.StopColima>();
        }
        
    }

    private static string AsGibibytes(long value) => $"{value / (double)gibibytesFactor} GiB";
}
