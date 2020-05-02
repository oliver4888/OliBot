using System;
using Serilog;
using System.IO;
using System.Linq;
using Common.Interfaces;
using System.Threading.Tasks;
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

            Type botCore = ModuleHelper.ModuleTypes.Where(module => module.GetInterfaces().Contains(typeof(IBotCoreModule))).FirstOrDefault();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

            if (botCore == null)
                logger.LogError("Unable to find a bot core implementation!");
            else
            {
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
