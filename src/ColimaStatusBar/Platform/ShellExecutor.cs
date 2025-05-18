using System.Diagnostics;
using ColimaStatusBar.Core.Platform;

namespace ColimaStatusBar.Platform;

internal sealed class ShellExecutor : IShellExecutor
{
    private static readonly string Shell =  Environment.GetEnvironmentVariable("SHELL") ?? "/bin/zsh";
    
    public async Task<ProcessResult> Run(string executable, string[] args, CancellationToken cancellationToken)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = Shell,
            Arguments = $"--login -c \"{executable} {string.Join(" ", args)}\"",
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
