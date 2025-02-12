using System.ComponentModel;
using System.Runtime.CompilerServices;
using Docker.DotNet;
using Docker.DotNet.Models;

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

    private Container[] containers = [];
    public Container[] Containers { get => containers; private set => SetField(ref containers, value, ArrayEqualityComparer<Container>.Instance); }

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

                if (isRunning)
                {
                    var rawContainers = await client.Containers.ListContainersAsync(new ContainersListParameters(), backgroundTask.Token);
                    var currentContainers = rawContainers.Select(c => new Container(c.ID, c.Names.First(), c.Image)).ToArray();
                    
                    Containers = currentContainers;
                }
                
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

    private void SetField<T>(ref T field, T value, IEqualityComparer<T>? equalityComparer = null, [CallerMemberName] string? propertyName = null)
    {
        var comparer = equalityComparer ?? EqualityComparer<T>.Default;
        if (comparer.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]> where T : IEquatable<T>
    {
        public static ArrayEqualityComparer<T> Instance { get; } = new();
        
        public bool Equals(T[]? x, T[]? y) => x is null ? y is null : y is not null && x.SequenceEqual(y);

        public int GetHashCode(T[] obj) => obj.Aggregate(obj.Length.GetHashCode(), HashCode.Combine);
    }
}
