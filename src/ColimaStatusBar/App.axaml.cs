using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;

namespace ColimaStatusBar;

public sealed partial class App : Application, IDisposable
{
    private ColimaInteractor? interactor;

    public static readonly FuncValueConverter<bool, string> StatusTextConverter = new(RenderStatus);
    public static readonly FuncValueConverter<bool, WindowIcon> StatusImageConverter = new(ChooseIcon);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        interactor = new ColimaInteractor(TimeSpan.FromSeconds(5));
        DataContext = interactor;
        interactor.Containers.CollectionChanged += OnContainersChanged;
        
        base.OnFrameworkInitializationCompleted();
    }

    private static string RenderStatus(bool isRunning)
    {
        return isRunning ? "Running" : "Stopped";
    }

    private static WindowIcon ChooseIcon(bool isRunning)
    {
        var isDark = Current?.ActualThemeVariant == ThemeVariant.Dark;
        var resourcePath = isDark
            ? (isRunning ? "Assets/cube-white.ico" : "Assets/cube-white-outline.ico")
            : (isRunning ? "Assets/cube.ico" : "Assets/cube-outline.ico");
        
        return new WindowIcon(AssetLoader.Open(new Uri($"avares://ColimaStatusBar/{resourcePath}")));
    }

    private void OnContainersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var tray = TrayIcon.GetIcons(this);
        if (tray is not [var icon])
        {
            return;
        }
        
        var menuItems = icon.Menu?.Items;
        if (menuItems is null)
        {
            return;
        }

        var toRemove = menuItems.Where(static i => i is NativeMenuItem { ToggleType: NativeMenuItemToggleType.CheckBox }).ToList();
        foreach (var item in toRemove)
        {
            menuItems.Remove(item);
        }

        foreach (var item in interactor?.Containers.ToList() ?? [])
        {
            var index = menuItems.IndexOf(menuItems.OfType<NativeMenuItemSeparator>().Last());
            var menuItem = new NativeMenuItem($"{item.Name}: {item.Image}") { ToggleType = NativeMenuItemToggleType.CheckBox, IsChecked = true };
            
            menuItems.Insert(index, menuItem);
        }
    }

    public void Dispose()
    {
        if (interactor is null)
        {
            return;
        }
        
        interactor.Containers.CollectionChanged -= OnContainersChanged;
        interactor.Dispose();
    }
}



