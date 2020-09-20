using Common;
using System;
using Serilog;
using System.IO;
using System.Linq;
using Common.Attributes;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotRunner
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration cmdLineConfig = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            string setEnvironmentName = cmdLineConfig[CommandLineFlags.SetEnvironmentName];

            if (setEnvironmentName != null && EnvironmentHelper.ValidEnvironments.Contains(setEnvironmentName))
                EnvironmentHelper.SetEnvironmentName(setEnvironmentName);
            else if (Debugger.IsAttached)
                EnvironmentHelper.SetEnvironmentName(Environments.Development);
            else
                EnvironmentHelper.SetEnvironmentName(Environments.Production);

            ModuleHelper.LoadModules(cmdLineConfig);

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{EnvironmentHelper.GetEnvironmentName()}.json", true);

            foreach (string file in ModuleHelper.PossibleConfigFiles)
                builder.AddJsonFile(file, true);

            IConfiguration configuration = builder
                .AddConfiguration(cmdLineConfig)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            await CreateHost(configuration).RunAsync();
        }

        static IHost CreateHost(IConfiguration configuration) => new HostBuilder()
            .UseEnvironment(EnvironmentHelper.GetEnvironmentName())
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
                    DependencyInjectedAttribute depAttr = type.GetCustomAttribute<DependencyInjectedAttribute>();
                    switch (depAttr.Type)
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
                            if (depAttr.Implements == null)
                                services.AddSingleton(type);
                            else
                                services.AddSingleton(depAttr.Implements, type);
                            continue;
                    }
                }
            })
            .Build();
    }
}
