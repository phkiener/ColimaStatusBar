using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Core.Platform;

namespace ColimaStatusBar.Core.Docker;

public sealed class DockerPollingJob(IShellExecutor shellExecutor, Action<RunningContainer[]> containerCallback) : BackgroundJob
{
    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(10);

    protected override async Task Run(CancellationToken cancellationToken)
    {
        var contextResponse = await shellExecutor.Run("docker", ["context", "ls", "--format", "'{{.Name}}'"], cancellationToken);
        if (contextResponse.ExitCode is not 0)
        {
            // Docker not installed? We don't really know, so just don't do anything.
            return;
        }

        var allContainers = new List<RunningContainer>();
        foreach (var context in contextResponse.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var containersResponse = await shellExecutor.Run(
                "docker",
                ["--context", context, "container", "ls", "--all", "--format", "'{{.ID}};{{.Image}};{{.State}};{{.Names}}'"],
                cancellationToken);
            
            if (containersResponse.ExitCode is not 0)
            {
                // The specific context is probably not running; just skip it.
                continue;
            }
            
            var containers = containersResponse.Output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(l => ParseContainer(l, context));
            
            allContainers.AddRange(containers);
        }
        
        containerCallback.Invoke(allContainers.ToArray());
    }

    private static RunningContainer ParseContainer(string line, string context)
    {
        var segments = line.Split(';');

        return new RunningContainer(
            Id: segments[0],
            Image: segments[1],
            Name: segments[3],
            Context: context,
            State: Enum.Parse<ContainerState>(segments[2], ignoreCase: true));
    }
}
