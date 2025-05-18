namespace ColimaStatusBar.Core.Platform;

public sealed record ProcessResult(int ExitCode, string Output);

public interface IShellExecutor
{
    Task<ProcessResult> Run(string executable, string[] args, CancellationToken cancellationToken);
}
