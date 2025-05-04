using System.Text.Json;
using System.Text.Json.Serialization;
using ColimaStatusBar.Framework;

namespace ColimaStatusBar.Core.Infrastructure;

public static class Colima
{
    public static async Task<RunningProfile?> StatusAsync(CancellationToken cancellationToken)
    {
        var (exitCode, output) = await ProcessRunner.RunAsShell("colima", ["status", "--json"], cancellationToken);
        if (exitCode is 1)
        {
            return null;
        }

        var parsedOutput = JsonSerializer.Deserialize<StatusOutput>(output);
        if (parsedOutput is null)
        {
            throw new FormatException($"Cannot parse output: {output}");
        }

        return new RunningProfile(
            Name: parsedOutput.Name,
            SocketAddress: parsedOutput.Socket,
            CpuCores: parsedOutput.Cpu,
            MemoryBytes: parsedOutput.Memory,
            DiskBytes: parsedOutput.Disk);
    }

    public static async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = await ProcessRunner.RunAsShell("colima", ["start"], cancellationToken);
    }

    public static async Task StopAsync(CancellationToken cancellationToken)
    {
        _ = await ProcessRunner.RunAsShell("colima", ["stop"], cancellationToken);
    }

    private sealed class StatusOutput
    {
        [JsonRequired]
        [JsonPropertyName("display_name")]
        public required string Name { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("docker_socket")]
        public required string Socket { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("cpu")]
        public required int Cpu { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("memory")]
        public required long Memory { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("disk")]
        public required long Disk { get; set; }
    }
}
