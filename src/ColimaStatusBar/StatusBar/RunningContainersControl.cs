using ColimaStatusBar.Core;
using ColimaStatusBar.Framework.AppKit;
using ColimaStatusBar.Framework.Flux;

namespace ColimaStatusBar.StatusBar;

public sealed class RunningContainersControl(RunningContainersStore store, Dispatcher dispatcher, Binder binder) : Control<NSMenu>(dispatcher, binder)
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
        var existingItems = menu.Items.OfType<RunningContainerItem>().ToList();
        foreach (var item in existingItems)
        {
            menu.RemoveItem(item);
            item.Dispose();
        }
        
        foreach (var container in store.RunningContainers.OrderByDescending(static c => c.Name))
        {
            var containerItem = new RunningContainerItem(container);
            containerItem.OnStart += async (_, _) => await Dispatcher.Invoke(new Commands.StartContainer(container.Id));
            containerItem.OnStop += async (_, _) => await Dispatcher.Invoke(new Commands.StopContainer(container.Id));
            containerItem.OnRemove += async (_, _) => await Dispatcher.Invoke(new Commands.RemoveContainer(container.Id));
            
            var location = menu.IndexOf(noContainersItem);
            menu.InsertItem(containerItem, location + 1);
        }
    }

    private void TogglePlaceholder(NSMenuItem item)
    {
        item.Hidden = store.RunningContainers.Any();
    }
}
