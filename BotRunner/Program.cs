using OliBot.API;
using System;
using Serilog;
using System.IO;
using System.Linq;
using OliBot.API.Attributes;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BotRunner
{
    static class Program
    {
        static readonly MethodInfo _addHostedService =
            typeof(ServiceCollectionHostedServiceExtensions).GetMethods().Single(mi =>
                mi.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService)
                && mi.GetParameters().Length == 1);

        static readonly MethodInfo _configureOptions =
            typeof(OptionsConfigurationServiceCollectionExtensions).GetMethods().Single(mi =>
                mi.Name == nameof(OptionsConfigurationServiceCollectionExtensions.Configure)
                && mi.GetParameters().Length == 2
                && mi.GetParameters()[1].ParameterType == typeof(IConfiguration));

        static readonly MethodInfo _addOptions = typeof(Program).GetMethod(nameof(AddOptions), BindingFlags.NonPublic | BindingFlags.Static);

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

                foreach (Type type in ModuleHelper.DependencyInjectedTypes)
                {
                    DependencyInjectedAttribute depAttr = type.GetCustomAttribute<DependencyInjectedAttribute>();
                    switch (depAttr.Type)
                    {
                        case DIType.HostedService:
                            _addHostedService.MakeGenericMethod(type).Invoke(services, new object[] { services });
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
                        case DIType.Options:
                            _configureOptions.MakeGenericMethod(type).Invoke(services, new object[] { services, configuration.GetSection(type.Name) });
                            _addOptions.MakeGenericMethod(type).Invoke(null, new object[] { services });
                            continue;
                    }
                }
            })
            .Build();

        static IServiceCollection AddOptions<T>(IServiceCollection services) where T : class, new() =>
            services.AddTransient(services => services.GetRequiredService<IOptions<T>>().Value);
    }
}
