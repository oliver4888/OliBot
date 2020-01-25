using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OliBot.Commands;
using OliBot.Utilities;

namespace OliBot
{
    static class Program
    {
        static void Main()
        {
            ModuleLoader.LoadModules();

            ConfigureServices(new ServiceCollection()).GetRequiredService<BotCore>().Start().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static IServiceProvider ConfigureServices(IServiceCollection services) =>
            services
                .AddConfiguration()
                .AddSingleton<ICommandManager, CommandManager>()
                .AddSingleton<BotCore>()
                .BuildServiceProvider();

        static IServiceCollection AddConfiguration(this IServiceCollection services) =>
            services.AddSingleton(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build());
    }
}
