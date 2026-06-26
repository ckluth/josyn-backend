using Asp.Versioning;
using Scalar.AspNetCore;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Wires up and returns a ready-to-run <see cref="WebApplication"/>.
/// Services are registered in <c>RegisterServices</c>; the HTTP pipeline is configured in
/// <c>ConfigurePipeline</c>.
/// </summary>
internal static class HostFactory
{
    internal static WebApplication Create(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        RegisterServices(builder.Services);

        var app = builder.Build();
        ConfigurePipeline(app);
        return app;

        // ── helpers ────────────────────────────────────────────────────────────
        static void RegisterServices(IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddOpenApi();
        }

        static void ConfigurePipeline(WebApplication app)
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.MapEndpoints();
        }
    }
}
