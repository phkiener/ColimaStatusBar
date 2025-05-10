using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class CurrentProfileItem : NSMenuItem
{
    private const long gibibytesFactor = 1073741824;
    
    private readonly Emitter emitter;
    private readonly ColimaStatusStore colimaStatus;

    public CurrentProfileItem(Emitter emitter, ColimaStatusStore colimaStatus)
    {
        this.emitter = emitter;
        this.colimaStatus = colimaStatus;
        
        Draw();
        emitter.OnEmit += OnEmit;
    }

    private void Draw()
    {
        Enabled = false;
        //Hidden = colimaStatus.CurrentProfile is null;

        if (colimaStatus.CurrentProfile is not null and var profile)
        {
            string[] detailInfoParts =
            [
                $"{profile.CpuCores} CPU",
                $"{AsGibibytes(profile.MemoryBytes)} RAM",
                $"{AsGibibytes(profile.DiskBytes)} Disk",
            ];

            Title = string.Join(" | ", detailInfoParts) + "\nAnother line?";
        }
        else
        {
            Title = "No profile running\nAnother line?";
        }
    }

    private void OnEmit(object? sender, INotification notification)
    {
        if (notification is ColimaProfileChanged)
        {
            InvokeOnMainThread(Draw);
        }
    }

    protected override void Dispose(bool disposing)
    {
        emitter.OnEmit -= OnEmit;

        base.Dispose(disposing);
    }

    private static string AsGibibytes(long value) => $"{value / (double)gibibytesFactor} GiB";
}
