using ColimaStatusBar.Core.Platform;
using ServiceManagement;

namespace ColimaStatusBar.Platform;

internal sealed class LoginItemsService : ILoginItemsService
{
    public bool LaunchAtLogin()
    {
        return SMAppService.MainApp.Status is SMAppServiceStatus.Enabled;
    }

    public async Task<bool> SetLaunchAtLogin(bool enabled)
    {
        if (enabled)
        {
            SMAppService.MainApp.Register();
            return true;
        }

        await SMAppService.MainApp.UnregisterAsync();
        return false;
    }
}
