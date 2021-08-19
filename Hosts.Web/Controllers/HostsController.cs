using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Environment;
using static System.IO.File;

namespace Hosts.Web.Controllers
{
    [ApiController]
    [Route("etc/hosts")]
    public class HostsController : ControllerBase
    {
        private static readonly Regex CnamePattern = new Regex("^cname=.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly ILogger _logger;
        private readonly AppSettings _settings;

        public HostsController(ILoggerFactory loggerFactory, AppSettings settings)
        {
            _logger = loggerFactory.CreateLogger("Hosts.Web");
            _settings = settings;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Text.Plain)]
        public async Task<string> GetAsync()
        {
            var hosts = await ReadAllTextAsync(_settings.HostsFilePath).ConfigureAwait(false);
            var cname = await ReadAllTextAsync(_settings.CnameFilePath).ConfigureAwait(false);
            return hosts.Trim() + NewLine + cname.Trime();
        }

        [HttpPut]
        [Consumes(MediaTypeNames.Text.Plain)]
        public async Task PutAsync([FromBody] string hosts)
        {
            var cname = string.Empty;
            hosts = CnamePattern.Replace(hosts, match =>
            {
                cname += NewLine + match.Value;
                return string.Empty;
            });
            await WriteAllTextAsync(_settings.HostsFilePath, hosts.Trim() + NewLine).ConfigureAwait(false);
            await WriteAllTextAsync(_settings.CnameFilePath, cname.Trim() + NewLine).ConfigureAwait(false);

            var si = new ProcessStartInfo().UseStandardIO().WithArguments(_settings.ReloadCommand);
            var (code, stdout, stderr) = await si.StartAsync().ConfigureAwait(false);
            if (code != 0)
                _logger.LogError(stdout + NewLine + stderr);
        }
    }
}
