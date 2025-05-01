using System.Diagnostics;

namespace ColimaStatusBar.Framework;

public sealed record ProcessResult(int ExitCode, string Output);

public static class ProcessRunner
{
    public static async Task<ProcessResult> RunProcessAsync(string executable, string[] args, CancellationToken cancellationToken)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            return new ProcessResult(255, "");
        }
        
        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        var exit = process.WaitForExitAsync(cancellationToken);
        await Task.WhenAll(output, error, exit);
        
        return new ProcessResult(process.ExitCode, await output);
    }
}
