using Hosts.Web;

var web = WebApplication.CreateBuilder(args);
web.Host.ConfigureServices(services =>
{
    services.AddControllers(controllers =>
    {
        controllers.InputFormatters.Add(new PlainTextInputFormatter());
    });
    services.Configure<AppSettings>(web.Configuration);
});

var app = web.Build();
app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.All })
   .UseDefaultFiles()
   .UseStaticFiles()
   .UseRouting()
   .UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();
