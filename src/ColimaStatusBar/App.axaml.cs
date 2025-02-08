using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace ColimaStatusBar;

public sealed partial class App : Application, IDisposable
{
    private ColimaInteractor? interactor;

    public static readonly FuncValueConverter<bool, string> StatusTextConverter = new(static b => b ? "Running" : "Stopped");
    public static readonly FuncValueConverter<bool, WindowIcon> StatusImageConverter = new(static b => b ? new WindowIcon(AssetLoader.Open(new Uri("avares://ColimaStatusBar/Assets/cube.ico"))) : new WindowIcon(AssetLoader.Open(new Uri("avares://ColimaStatusBar/Assets/cube-outline.ico"))));

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        interactor = new ColimaInteractor(TimeSpan.FromSeconds(5));
        DataContext = interactor;
        
        base.OnFrameworkInitializationCompleted();
    }

    public void Dispose()
    {
        interactor?.Dispose();
    }
}



