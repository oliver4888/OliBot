using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotRunner
{
    static class Program
    {
        static void Main(string[] args)
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

            Type botCore = ModuleHelper.ModuleTypes.Where(module => module.Name == "BotCoreModule").FirstOrDefault();
            (botCore.GetMethod("Start").Invoke(services.BuildServiceProvider().GetRequiredService(botCore), null) as Task).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
