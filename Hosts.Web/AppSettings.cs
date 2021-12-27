namespace Hosts.Web;

public class AppSettings
{
    public string                       HostsFilePath { get; set; } = "/etc/hosts";
    public string?                      CnameFilePath { get; set; }
    public IReadOnlyCollection<string>? ReloadCommand { get; set; }
}
