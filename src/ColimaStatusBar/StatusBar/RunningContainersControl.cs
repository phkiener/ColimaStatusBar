using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using Swallow.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class RunningContainersControl(RunningContainersStore store, IDispatcher dispatcher, IBinder binder) : Control<NSMenu>(dispatcher, binder)
{
    private readonly NSMenuItem noContainersItem = new("No containers running") { Enabled = false, Hidden = true };
    
    protected override void OnAttach(NSMenu target)
    {
        target.AddItem(noContainersItem);
        Binder.BindControl(target).To<RunningContainersChanged>(UpdateContainers);
        Binder.BindControl(noContainersItem).To<RunningContainersChanged>(TogglePlaceholder);
    }

    private void UpdateContainers(NSMenu menu)
    {
        var runningContainers = store.RunningContainers.OrderByDescending(static c => c.Name).ToList();
        foreach (var container in runningContainers)
        {
            var existingItem = menu.Items.OfType<RunningContainerItem>().SingleOrDefault(i => i.ContainerId == container.Id);
            if (existingItem is not null && existingItem.Container != container)
            {
                menu.RemoveItem(existingItem);
                existingItem.Dispose();
                existingItem = null;
            }

            if (existingItem is null)
            {
                var containerItem = new RunningContainerItem(container);
                containerItem.OnStart += async (_, _) => await Dispatcher.Dispatch(new Commands.StartContainer(container.Id));
                containerItem.OnStop += async (_, _) => await Dispatcher.Dispatch(new Commands.StopContainer(container.Id));
                containerItem.OnRemove += async (_, _) => await Dispatcher.Dispatch(new Commands.RemoveContainer(container.Id));
                
                var location = menu.IndexOf(noContainersItem);
                menu.InsertItem(containerItem, location + 1);
            }
        }
        
        var leftoverItems = menu.Items.OfType<RunningContainerItem>().Where(i => runningContainers.All(c => c.Id != i.ContainerId)).ToList();
        foreach (var item in leftoverItems)
        {
            menu.RemoveItem(item);
            item.Dispose();
        }
    }

    private void TogglePlaceholder(NSMenuItem item)
    {
        item.Hidden = store.RunningContainers.Any();
    }
}
