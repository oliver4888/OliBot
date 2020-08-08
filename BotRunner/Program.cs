using System;
using Serilog;
using System.IO;
using Common.Attributes;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BotRunner
{
    static class Program
    {
        static async Task Main()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            ModuleHelper.LoadModules();

            await new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddSingleton(configuration)
                        .AddHostedService<Worker>()
                        .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

                    foreach (Type module in ModuleHelper.ModuleTypes)
                    {
                        Type implements = module.GetCustomAttribute<ModuleAttribute>().Implements;
                        if (implements == null)
                            services.AddSingleton(module);
                        else
                            services.AddSingleton(implements, module);
                    }
                })
                .RunConsoleAsync();
        }
    }
}
