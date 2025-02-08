using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ColimaStatusBar;

public sealed class ColimaInteractor : INotifyPropertyChanged, IDisposable
{
    private readonly CancellationTokenSource backgroundTask = new();
    private bool isRunning;
    private readonly TimeSpan refreshInterval;

    public ColimaInteractor(TimeSpan refreshInterval)
    {
        this.refreshInterval = refreshInterval;
        Task.Run(RefreshStatus, backgroundTask.Token);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
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
                await timer.WaitForNextTickAsync(backgroundTask.Token);
                IsRunning = !isRunning;
            }
            catch
            {
                // Ignore it.
            }
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public void Dispose()
    {
        backgroundTask.Cancel();
    }
}
