using Common;
using System;
using Serilog;
using System.IO;
using System.Linq;
using Common.Attributes;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                        .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

                    MethodInfo addHostedServiceMethod =
                        typeof(ServiceCollectionHostedServiceExtensions).GetMethods()
                            .Single(mi =>
                                mi.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService)
                                && mi.GetParameters().Length == 1);

                    foreach (Type type in ModuleHelper.DependencyInjectedTypes)
                    {
                        switch (type.GetCustomAttribute<DependencyInjectedAttribute>().Type)
                        {
                            case DIType.HostedService:
                                addHostedServiceMethod.MakeGenericMethod(type).Invoke(services, new object[] { services });
                                continue;
                            case DIType.Transient:
                                services.AddTransient(type);
                                continue;
                            case DIType.Scoped:
                                services.AddScoped(type);
                                continue;
                            case DIType.Singleton:
                                Type implements = type.GetCustomAttribute<ModuleAttribute>()?.Implements;
                                if (implements == null)
                                    services.AddSingleton(type);
                                else
                                    services.AddSingleton(implements, type);
                                continue;
                        }
                    }
                })
                .RunConsoleAsync();
        }
    }
}
