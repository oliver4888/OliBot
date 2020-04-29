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
            IEnumerable<Type> modules = ModuleHelper.GetModules();
            Type botCore = modules.Where(module => module.Name == "BotCoreModule").FirstOrDefault();

            foreach (Type module in modules)
                services.AddSingleton(module);

            (botCore.GetMethod("Start").Invoke(services.BuildServiceProvider().GetRequiredService(botCore), null) as Task).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static IServiceCollection AddConfiguration(this IServiceCollection services) =>
            services.AddSingleton(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build());
    }
}
