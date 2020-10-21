using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Environment;

namespace System.Diagnostics
{
    public static class ProcessStartInfoExtensions
    {
        public static ProcessStartInfo WithArguments(this ProcessStartInfo startInfo, IEnumerable<string> arguments)
        {
            foreach (var (i, argument) in arguments.Select((a, i) => (i, a)))
            {
                if (i == 0 && string.IsNullOrEmpty(startInfo.FileName))
                    startInfo.FileName = argument;
                else
                    startInfo.ArgumentList.Add(argument);
            }
            return startInfo;
        }

        public static ProcessStartInfo UseStandardIO(this ProcessStartInfo startInfo)
        {
            startInfo.UseShellExecute        = false;
            startInfo.CreateNoWindow         = true;
            startInfo.RedirectStandardInput  = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError  = true;
            return startInfo;
        }

        public static async Task<(int Code, string Output, string Error)> StartAsync(this ProcessStartInfo startInfo)
        {
            int code = 0;
            var stdout = new ConcurrentQueue<string>();
            var stderr = new ConcurrentQueue<string>();
            using (var process = Process.Start(startInfo))
            {
                var tcs = new TaskCompletionSource<int>();
                process.EnableRaisingEvents = true;
                process.Exited             += (_, e) => tcs.TrySetResult(process.ExitCode);
                process.OutputDataReceived += (_, e) => stdout.Enqueue(e.Data);
                process.ErrorDataReceived  += (_, e) => stderr.Enqueue(e.Data);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (process.HasExited)
                    tcs.TrySetResult(process.ExitCode);
                code = await tcs.Task.ConfigureAwait(false);
            }
            return (code, string.Join(NewLine, stdout).Trim(), string.Join(NewLine, stderr).Trim());
        }
    }
}
