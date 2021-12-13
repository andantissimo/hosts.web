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

        var hosts = await ReadAllTextAsync(settings.HostsFilePath).ConfigureAwait(false);
        var cname = await ReadAllTextAsync(settings.CnameFilePath).ConfigureAwait(false);
        return hosts.Trim() + NewLine + cname.Trim();
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
        await WriteAllTextAsync(settings.HostsFilePath, hosts.Trim() + NewLine).ConfigureAwait(false);
        await WriteAllTextAsync(settings.CnameFilePath, cname.Trim() + NewLine).ConfigureAwait(false);

        List<string> stdout = new(), stderr = new();
        var si = new ProcessStartInfo(settings.ReloadCommand.First())
        {
            CreateNoWindow         = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
        };
        si.ArgumentList.AddRange(settings.ReloadCommand.Skip(1));
        using var process = new Process { StartInfo = si };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is string line)
                stdout.Add(line);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is string line)
                stderr.Add(line);
        };
        var exited = process.StartAsync();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var code = await exited.ConfigureAwait(false);
        process.CancelOutputRead();
        process.CancelErrorRead();
        if (code != 0)
            _logger.LogError("{output}", string.Join(NewLine, stdout.Concat(stderr)));
    }
}
