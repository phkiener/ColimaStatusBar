using System.Text.Json;
using System.Text.Json.Serialization;
using ColimaStatusBar.Core.Abstractions;
using ColimaStatusBar.Core.Platform;

namespace ColimaStatusBar.Core.Colima;

public sealed class ColimaPollingJob(IShellExecutor shellExecutor, Action<ColimaProfileInfo[]> profileCallback) : BackgroundJob
{
    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);

    protected override async Task Run(CancellationToken token)
    {
        var result = await shellExecutor.Run("colima", ["list", "-j"], token);
        if (result.ExitCode is not 0)
        {
            // Colima not installed? We don't really know, so just don't do anything.
            return;
        }

        var profiles = result.Output.Split('\n')
            .Select(ParseProfile)
            .ToArray();
        
        profileCallback.Invoke(profiles);
    }

    private static ColimaProfileInfo ParseProfile(string line)
    {
        var output = JsonSerializer.Deserialize<ColimaListOutput>(line);
        if (output is null)
        {
            throw new FormatException($"Cannot parse '{line}' as profile");
        }

        return new ColimaProfileInfo(
            Name: output.Name,
            Status: Enum.Parse<ProfileStatus>(output.Status),
            CpuCount: output.CpuCount,
            MemoryBytes: output.Memory,
            DiskBytes: output.Disk);
    }

    private sealed class ColimaListOutput
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = "";
        
        [JsonPropertyName("status")]
        public string Status { get; init; } = "";
        
        [JsonPropertyName("cpus")]
        public int CpuCount { get; init; } = 0;
        
        [JsonPropertyName("memory")]
        public long Memory { get; init; } = 0;
        
        [JsonPropertyName("disk")]
        public long Disk { get; init; } = 0;
    }
}
