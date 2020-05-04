using System;
using Serilog;
using System.IO;
using System.Linq;
using System.Text;
using Common.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BotRunner
{
    static class Program
    {
        static void Main()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            IServiceCollection services = new ServiceCollection()
                .AddSingleton(configuration)
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            ModuleHelper.LoadModules();

            foreach (Type module in ModuleHelper.ModuleTypes)
                services.AddSingleton(module);

            IEnumerable<Type> botCoreModules = ModuleHelper.ModuleTypes.Where(module => module.GetInterfaces().Contains(typeof(IBotCoreModule)));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

            if (!botCoreModules.Any())
                logger.LogError("Unable to find a bot core implementation!");
            else if (botCoreModules.Count() != 1)
            {
                StringBuilder errorBuilder = new StringBuilder()
                    .AppendLine($"More than one implementation of {nameof(IBotCoreModule)} was found!");

                foreach (Type module in botCoreModules)
                    errorBuilder.AppendLine($"{module.Name} in {module.Assembly.FullName}");

                errorBuilder.AppendLine($"Please remove extra implementations of {nameof(IBotCoreModule)} before running the bot.");
                logger.LogError(errorBuilder.ToString());
            }
            else
            {
                // Fetch each module from the DI container to load them
                foreach (Type module in ModuleHelper.ModuleTypes.Where(module => !module.GetInterfaces().Contains(typeof(IBotCoreModule))))
                    serviceProvider.GetRequiredService(module);

                Type botCore = botCoreModules.First();
                IBotCoreModule botCoreModule = serviceProvider.GetRequiredService(botCore) as IBotCoreModule;
                try
                {
                    (botCore.GetMethod("Start").Invoke(botCoreModule, null) as Task).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error starting bot!");
                }
            }
        }
    }
}
