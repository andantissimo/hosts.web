using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Mime;
using System.Threading.Tasks;
using static System.Environment;
using static System.IO.File;

namespace Hosts.Web.Controllers
{
    [ApiController]
    [Route("etc/hosts")]
    public class HostsController : ControllerBase
    {
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
            return await ReadAllTextAsync(_settings.HostsFilePath).ConfigureAwait(false);
        }

        [HttpPut]
        [Consumes(MediaTypeNames.Text.Plain)]
        public async Task PutAsync([FromBody] string hosts)
        {
            await WriteAllTextAsync(_settings.HostsFilePath, hosts).ConfigureAwait(false);

            var si = new ProcessStartInfo().UseStandardIO().WithArguments(_settings.ReloadCommand);
            var (code, stdout, stderr) = await si.StartAsync().ConfigureAwait(false);
            if (code != 0)
                _logger.LogError(stdout + NewLine + stderr);
        }
    }
}
