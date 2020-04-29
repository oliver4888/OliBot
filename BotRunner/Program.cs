using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotRunner
{
    static class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddConfiguration();

            ModuleHelper.LoadModules();

            foreach (Type module in ModuleHelper.ModuleTypes)
                services.AddSingleton(module);

            Type botCore = ModuleHelper.ModuleTypes.Where(module => module.Name == "BotCoreModule").FirstOrDefault();
            (botCore.GetMethod("Start").Invoke(services.BuildServiceProvider().GetRequiredService(botCore), null) as Task).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static IServiceCollection AddConfiguration(this IServiceCollection services) =>
            services.AddSingleton(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build());
    }
}
