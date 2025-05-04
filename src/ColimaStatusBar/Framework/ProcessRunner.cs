using System.Diagnostics;

namespace ColimaStatusBar.Framework;

public sealed record ProcessResult(int ExitCode, string Output);

public static class ProcessRunner
{
    private static readonly string Shell =  Environment.GetEnvironmentVariable("SHELL") ?? "/bin/zsh";
    
    public static async Task<ProcessResult> RunAsShell(string executable, string[] args, CancellationToken cancellationToken)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = Shell,
            Arguments = $"-r --login -c \"{executable} {string.Join(" ", args)}\"",
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
