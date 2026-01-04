using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VerifileChallengeApp.Database;
using VerifileChallengeApp.HttpClients.TWFU;
using Verifile.PubSub;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .Configure<TWFUSettings>(builder.Configuration.GetSection(TWFUSettings.SectionName));

builder.Services
    .AddHttpClient<TWFUService>((serviceProvider, client) =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<TWFUSettings>>().Value;

        client.BaseAddress = new Uri(settings.BaseUrl);
    });

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddDbContext<VerifileDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")))
    .AddPubSub();

var build = builder.Build();

using (var scope = build.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VerifileDbContext>();

    await db.Database.MigrateAsync();
}

await build.RunAsync();
