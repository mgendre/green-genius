namespace GreenGenius.App.Extensions;

public static class StartupExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
    }
}