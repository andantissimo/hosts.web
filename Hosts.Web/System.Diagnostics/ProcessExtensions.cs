namespace System.Diagnostics;

internal static class ProcessExtensions
{
    public static async Task<int> StartAsync(this Process process, CancellationToken cancellationToken = default)
    {
        var exited = new TaskCompletionSource<int>();
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => exited.TrySetResult(process.ExitCode);
        process.Start();
        if (process.HasExited)
            return process.ExitCode;
        using var _ = cancellationToken.Register(() =>
        {
            if (process.Id != default)
                process.Kill(true);
            exited.TrySetCanceled(cancellationToken);
        });
        return await exited.Task.ConfigureAwait(false);
    }
}
