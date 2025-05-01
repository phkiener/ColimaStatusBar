using System.Text.Json;
using System.Text.Json.Serialization;
using ColimaStatusBar.Framework;

namespace ColimaStatusBar.Core.Infrastructure;

public static class Colima
{
    private static string? colimaPath = null;
    
    public static async Task<RunningProfile?> StatusAsync(CancellationToken cancellationToken)
    {
        colimaPath ??= await DetermineColimaPathAsync(cancellationToken);

        var (exitCode, output) = await ProcessRunner.RunProcessAsync(colimaPath, ["status", "--json"], cancellationToken);
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
    

    private static async Task<string> DetermineColimaPathAsync(CancellationToken cancellationToken)
    {
        var (_, path) = await ProcessRunner.RunProcessAsync("/bin/sh", ["-c", "\"which colima\""], cancellationToken);

        return path.Trim();
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
