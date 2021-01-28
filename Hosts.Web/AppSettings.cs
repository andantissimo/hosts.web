using System.Collections.Generic;

namespace Hosts.Web
{
    public class AppSettings
    {
        public string                      HostsFilePath { get; set; }
        public string                      CnameFilePath { get; set; }
        public IReadOnlyCollection<string> ReloadCommand { get; set; }
    }
}
