using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OliBot.Commands;

namespace OliBot
{
    static class Program
    {
        static void Main() => ConfigureServices().GetRequiredService<BotCore>().Start().ConfigureAwait(false).GetAwaiter().GetResult();

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
