namespace Hosts.Web;

public class AppSettings
{
    public string                      HostsFilePath { get; set; } = default!;
    public string                      CnameFilePath { get; set; } = default!;
    public IReadOnlyCollection<string> ReloadCommand { get; set; } = default!;
}
