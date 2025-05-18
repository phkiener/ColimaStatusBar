namespace ColimaStatusBar.Core.Platform;

public interface ILoginItemsService
{
    bool LaunchAtLogin();

    Task<bool> SetLaunchAtLogin(bool enabled);
}
