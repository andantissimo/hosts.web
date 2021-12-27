using Hosts.Web;

var web = WebApplication.CreateBuilder(args);
web.Services.AddControllers(controllers =>
{
    controllers.InputFormatters.Add(new PlainTextInputFormatter());
});
web.Services.Configure<AppSettings>(web.Configuration);

var app = web.Build();
app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.All })
   .UseDefaultFiles()
   .UseStaticFiles()
   .UseRouting()
   .UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();
