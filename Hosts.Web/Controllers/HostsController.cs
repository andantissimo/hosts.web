using static System.Environment;
using static System.IO.File;

namespace Hosts.Web.Controllers;

[ApiController]
[Route("etc/hosts")]
public class HostsController : ControllerBase
{
    private static readonly Regex CnamePattern = new("^cname=.*$", RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly ILogger _logger;
    private readonly IOptions<AppSettings> _settings;

    public HostsController(ILoggerFactory loggerFactory, IOptions<AppSettings> settings)
    {
        _logger = loggerFactory.CreateLogger("Hosts.Web");
        _settings = settings;
    }

    [HttpGet]
    [Produces(MediaTypeNames.Text.Plain)]
    public async Task<string> GetAsync()
    {
        var settings = _settings.Value;

        var tasks = new[] { settings.HostsFilePath, settings.CnameFilePath }
            .Where(path => Exists(path))
            .Select(path => ReadAllTextAsync(path!));
        var texts = await Task.WhenAll(tasks).ConfigureAwait(false);

        var builder = new StringBuilder();
        foreach (var text in texts)
        {
            if (!string.IsNullOrWhiteSpace(text))
                builder.AppendLine(text.Trim());
        }
        return builder.ToString();
    }

    [HttpPut]
    [Consumes(MediaTypeNames.Text.Plain)]
    public async Task PutAsync([FromBody] string hosts)
    {
        var settings = _settings.Value;

        var cname = string.Empty;
        hosts = CnamePattern.Replace(hosts, match =>
        {
            cname += NewLine + match.Value;
            return string.Empty;
        });
        var tasks = new (string? Path, string Text)[] { (settings.HostsFilePath, hosts), (settings.CnameFilePath, cname) }
            .Where(it => it.Path is not null)
            .Select(it => WriteAllTextAsync(it.Path!, it.Text.Trim() + NewLine));
        await Task.WhenAll(tasks).ConfigureAwait(false);

        if (settings.ReloadCommand is { Count: > 0 })
        {
            var (code, stdout, stderr) = await ExecuteAsync(settings.ReloadCommand).ConfigureAwait(false);
            if (code != 0)
                _logger.LogError("{Error}", (stdout + NewLine + stderr).Trim());
        }
    }

    private static async Task<(int ExitCode, string Output, string Error)> ExecuteAsync(IEnumerable<string> args)
    {
        StringBuilder stdout = new(), stderr = new();
        var startInfo = new ProcessStartInfo(args.First())
        {
            CreateNoWindow         = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
        };
        startInfo.ArgumentList.AddRange(args.Skip(1));
        using var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is string line)
                stdout.AppendLine(line);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is string line)
                stderr.AppendLine(line);
        };
        var exited = process.StartAsync();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var code = await exited.ConfigureAwait(false);
        process.CancelOutputRead();
        process.CancelErrorRead();
        return (code, stdout.ToString().Trim(), stderr.ToString().Trim());
    }
}
