using Common;
using System;
using System.Linq;
using System.Threading;
using Common.Attributes;
using Common.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BotRunner
{
    [DependencyInjected(DIType.HostedService)]
    public class Worker : BackgroundService
    {
        readonly ILogger<Worker> _logger;
        readonly IServiceProvider _serviceProvider;
        readonly IBotCoreModule _botCoreModule;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IBotCoreModule botCoreModule) =>
            (_logger, _serviceProvider, _botCoreModule) = (logger, serviceProvider, botCoreModule);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Fetch each module from the DI container to load them
                foreach (Type module in ModuleHelper.DependencyInjectedTypes.Where(type => 
                    type.Name.EndsWith("Module") && type.IsDefined(typeof(ModuleAttribute), false) && !type.GetInterfaces().Contains(typeof(IBotCoreModule))))
                    _serviceProvider.GetRequiredService(module);

                await _botCoreModule.Start();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error running bot.");
            }
        }
    }
}
