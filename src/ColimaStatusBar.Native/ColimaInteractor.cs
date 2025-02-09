using System.ComponentModel;
using System.Runtime.CompilerServices;
using Docker.DotNet;

namespace ColimaStatusBar;

public sealed record Container(string Id, string Name, string Image);

public sealed class ColimaInteractor : INotifyPropertyChanged, IDisposable
{
    private readonly CancellationTokenSource backgroundTask = new();
    private readonly TimeSpan refreshInterval;

    public ColimaInteractor(TimeSpan refreshInterval)
    {
        this.refreshInterval = refreshInterval;
        Task.Run(RefreshStatus, backgroundTask.Token);
    }

    private bool isRunning;
    public bool IsRunning { get => isRunning; private set => SetField(ref isRunning, value); }

    public void Quit()
    {
        Environment.Exit(0);
    }
    
    private async void RefreshStatus()
    {
        using var timer = new PeriodicTimer(refreshInterval);
        while (!backgroundTask.IsCancellationRequested)
        {
            try
            {
                using var clientConfiguration = Environment.GetEnvironmentVariable("DOCKER_HOST") is { } uri
                    ? new DockerClientConfiguration(new Uri(uri))
                    : new DockerClientConfiguration();

                using var client = clientConfiguration.CreateClient();

                try
                {
                    await client.System.PingAsync(backgroundTask.Token);
                    IsRunning = true;
                }
                catch (DockerApiException)
                {
                    IsRunning = false;
                }

                //if (isRunning)
                //{
                //    var rawContainers = await client.Containers.ListContainersAsync(new ContainersListParameters(), backgroundTask.Token);
                //    foreach (var rawContainer in rawContainers)
                //    {
                //        var container = new Container(rawContainer.ID, rawContainer.Names.First(), rawContainer.Image);
                //        var matchingContainer = containers.SingleOrDefault(c => c.Id == container.Id);
                //        if (matchingContainer is null)
                //        {
                //            Dispatcher.UIThread.Invoke(() => containers.Add(container));
                //        }
                //        else if (matchingContainer.Image != container.Image || matchingContainer.Name != container.Name)
                //        {
                //            Dispatcher.UIThread.Invoke(() => containers.Remove(matchingContainer));
                //            Dispatcher.UIThread.Invoke(() => containers.Add(container));
                //        }
                //    }
                //
                //    var obsoleteContainers = containers.Where(c => rawContainers.All(rc => rc.ID != c.Id)).ToList();
                //    foreach (var container in obsoleteContainers)
                //    {
                //        Dispatcher.UIThread.Invoke(() => containers.Remove(container));
                //    }
                //}
                
                await timer.WaitForNextTickAsync(backgroundTask.Token);
            }
            catch
            {
                // Some error handling would be nice, eh?
            }
        }
    }

    public void Dispose()
    {
        backgroundTask.Cancel();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
