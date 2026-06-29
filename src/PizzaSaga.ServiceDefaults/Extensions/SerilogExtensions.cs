using Microsoft.AspNetCore.Builder;
using Serilog;

namespace PizzaSaga.ServiceDefaults.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogDefaults(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, services, lc) =>
        {
            lc.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext();
        });

        return builder;
    }
}