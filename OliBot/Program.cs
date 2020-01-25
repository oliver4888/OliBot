using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OliBot.Commands;

namespace OliBot
{
    static class Program
    {
        static void Main()
        {
            string moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), "Modules");

            if (!Directory.Exists(moduleFolder))
                Directory.CreateDirectory(moduleFolder);

            IEnumerable<string> modules = Directory.EnumerateFiles(moduleFolder);

            foreach (string module in modules)
                Assembly.LoadFile(module);

            ConfigureServices().GetRequiredService<BotCore>().Start().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services
                .AddConfiguration()
                .AddSingleton<ICommandManager, CommandManager>()
                .AddSingleton<BotCore>();

            return services.BuildServiceProvider();
        }

        static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            services.AddSingleton(configuration);
            return services;
        }
    }
}
