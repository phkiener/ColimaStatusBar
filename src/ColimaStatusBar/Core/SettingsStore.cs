using ColimaStatusBar.Framework.Flux;
using ServiceManagement;

namespace ColimaStatusBar.Core;

public sealed record LaunchAtLoginChanged : INotification;

public sealed class SettingsStore(Emitter emitter) : IStore
{
    public bool StartAtLogin { get; private set; } = false;

    Task IStore.Handle(ICommand command)
    {
        if (command is Commands.Initialize)
        {
            StartAtLogin = SMAppService.MainApp.Status is SMAppServiceStatus.Enabled;
            emitter.Emit<LaunchAtLoginChanged>();

            return Task.CompletedTask;
        }

        if (command is Commands.LaunchAtLogin { Enabled: var doLaunchAtLogin } && doLaunchAtLogin != StartAtLogin)
        {
            if (doLaunchAtLogin)
            {
                SMAppService.MainApp.Register();
            }
            else
            {
                SMAppService.MainApp.Unregister();
            }

            StartAtLogin = doLaunchAtLogin;
            emitter.Emit<LaunchAtLoginChanged>();
        }

        return Task.CompletedTask;
    }
}
