using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Core.Platform;
using Swallow.Flux;

namespace ColimaStatusBar.Core.Settings;

public sealed class SettingsStore : AbstractStore, ISettings
{
    private readonly ILoginItemsService loginItemsService;

    public SettingsStore(ILoginItemsService loginItemsService, IEmitter emitter) : base(emitter)
    {
        this.loginItemsService = loginItemsService;
        
        Register<Commands.Initialize>(OnInitialize);
        Register<Commands.LaunchAtLogin>(OnToggle);
    }

    public bool LaunchAtLogin { get; private set; }

    private void OnInitialize()
    {
        LaunchAtLogin = loginItemsService.LaunchAtLogin();
        Emit(new LaunchAtLoginChanged(LaunchAtLogin));
    }

    private async Task OnToggle(Commands.LaunchAtLogin command)
    {
        await loginItemsService.SetLaunchAtLogin(command.Enabled);

        LaunchAtLogin = command.Enabled;
        Emit(new LaunchAtLoginChanged(LaunchAtLogin));
    }
}
