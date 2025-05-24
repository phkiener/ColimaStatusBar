using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Ui.Framework;
using Swallow.Flux;

namespace ColimaStatusBar.Ui.Controls;

public sealed class RunningContainersControl(IDocker docker, IDispatcher dispatcher, IBinder binder) : Control<NSMenu>(dispatcher, binder)
{
    private readonly NSMenuItem noContainersItem = new("No containers running") { Enabled = false, Hidden = true };
    private readonly Dictionary<string, NSMenuItem> containers = new();
    
    protected override void OnAttach(NSMenu target)
    {
        target.AddItem(noContainersItem);
        
        Binder.BindControl(target)
            .To<ContainerAdded>(OnContainerAdded)
            .To<ContainerRemoved>(OnContainerRemoved)
            .To<ContainerStatusChanged>(OnContainerStatusChanged);
        
        Binder.BindControl(noContainersItem)
            .To<ContainerAdded>(UpdatePlaceholderItem)
            .To<ContainerRemoved>(UpdatePlaceholderItem)
            .To<ContainerStatusChanged>(UpdatePlaceholderItem);

        UpdatePlaceholderItem(noContainersItem);
        foreach (var container in docker.RunningContainers)
        {
            AddContainer(target, container);
        }
    }

    private void OnContainerAdded(NSMenu menu, ContainerAdded containerAdded)
    {
        if (containers.ContainsKey(containerAdded.Id))
        {
            return;
        }

        var container = docker.GetContainer(containerAdded.Id);
        if (container is null)
        {
            return;
        }

        AddContainer(menu, container);
    }

    private void OnContainerRemoved(NSMenu menu, ContainerRemoved containerRemoved)
    {
        if (!containers.ContainsKey(containerRemoved.Id))
        {
            return;
        }
        
        var menuItem = containers.GetValueOrDefault(containerRemoved.Id);
        if (menuItem is null)
        {
            return;
        }

        containers.Remove(containerRemoved.Id);

        menu.RemoveItem(menuItem);
        menuItem.Dispose();
    }

    private void OnContainerStatusChanged(NSMenu menu, ContainerStatusChanged statusChanged)
    {
        var menuItem = containers.GetValueOrDefault(statusChanged.Id);
        var container = docker.GetContainer(statusChanged.Id);

        if (menuItem is not null && container is not null)
        {
            UpdateContainerState(menuItem, container);
        }
    }

    private void AddContainer(NSMenu menu, RunningContainer container)
    {
        var menuItem = new NSMenuItem($"{container.Name} ({container.State})") { Submenu = new NSMenu() };
        menuItem.Submenu.AddItem(new NSMenuItem($"Name: {container.Name}") { Enabled = false });
        menuItem.Submenu.AddItem(new NSMenuItem($"Image: {container.Image}") { Enabled = false });
        menuItem.Submenu.AddItem(new NSMenuItem($"State: {container.State}") { Enabled = false });
        menuItem.Submenu.AddItem(NSMenuItem.SeparatorItem);
        menuItem.Submenu.AddItem(new NSMenuItem("Copy container name", (_, _) => CopyToClipboard(container.Name)));
        menuItem.Submenu.AddItem(new NSMenuItem("Start", (_, _) => Dispatcher.Dispatch(new Commands.StartContainer(container.Id))));
        menuItem.Submenu.AddItem(new NSMenuItem("Stop", (_, _) => Dispatcher.Dispatch(new Commands.StopContainer(container.Id))));
        menuItem.Submenu.AddItem(new NSMenuItem("Remove", (_, _) => Dispatcher.Dispatch(new Commands.RemoveContainer(container.Id))));
        
        containers.Add(container.Id, menuItem);

        var index = menu.IndexOf(noContainersItem);
        menu.InsertItem(menuItem, index);

        UpdateContainerState(menuItem, container);
    }

    private void UpdateContainerState(NSMenuItem item, RunningContainer container)
    {
        item.Title = $"{container.Name} ({container.State})";
        if (item.Submenu is null)
        {
            return; // weird, but let's not crash
        }

        item.Submenu.Items[2].Title = $"State: {container.State}";
        item.Submenu.Items[5].Hidden = !container.CanStart;
        item.Submenu.Items[6].Hidden = !container.CanStop;
        item.Submenu.Items[7].Hidden = !container.CanRemove;
    }

    private void UpdatePlaceholderItem(NSMenuItem item)
    {
        item.Hidden = docker.RunningContainers.Any();
    }
    
    private static void CopyToClipboard(string text)
    {
        NSPasteboard.GeneralPasteboard.ClearContents();
        NSPasteboard.GeneralPasteboard.SetDataForType(NSData.FromString(text), NSPasteboardType.String.GetConstant());
    }
}
