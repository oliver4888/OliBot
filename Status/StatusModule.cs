using System;
using System.IO;
using System.Timers;
using System.Text.Json;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Status
{
    [Module]
    public class StatusModule
    {
        internal static ILogger<StatusModule> _logger;
        internal static IBotCoreModule _botCoreModule;

        const string _statusConfigFileName = "StatusModuleConfig.json";
        static string _configFile;

        internal readonly static Timer StatusTimer = new Timer(30 * (60 * 1000)); // Every 30 minutes
        static readonly Random _random = new Random();

        internal static StatusModuleConfig StatusConfig { get; set; }

        public StatusModule(ILogger<StatusModule> logger, IBotCoreModule botCoreModule)
        {
            _logger = logger;
            _botCoreModule = botCoreModule;

            _botCoreModule.CommandHandler.RegisterCommands<StatusCommands>();
            _botCoreModule.DiscordClient.Ready += async (client, e) => await SetRandomStatus();

            _configFile = Path.Combine(Environment.CurrentDirectory, _statusConfigFileName);

            if (!File.Exists(_configFile))
            {
                _logger.LogInformation($"{nameof(StatusModule)}: No config file found. Creating default config at {_configFile}");
                StatusConfig = new StatusModuleConfig
                {
                    Mode = StatusMode.Automatic
                };
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AddStatus(ActivityType.ListeningTo, "Rick Astley", false);
                AddStatus(ActivityType.Playing, "Minecraft");
            }
            else
                LoadConfig();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            StatusTimer.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                await SetRandomStatus();
            };
            StatusTimer.Start();
        }

        internal static bool IsValidStatus(ActivityType type, string message) =>
            !(type == ActivityType.Streaming || type == ActivityType.Custom || string.IsNullOrWhiteSpace(message));

        internal static async Task SetStatus(ActivityType type, string message) =>
            await _botCoreModule.DiscordClient.UpdateStatusAsync(new DiscordActivity(message, type));

        internal static async Task<bool> AddStatus(ActivityType type, string message, bool save = true)
        {
            if (!IsValidStatus(type, message))
                return false;

            StatusConfig.Statuses.Add(new Status { ActivityType = type, Message = message });

            if (save)
                await SaveConfig();
            return true;
        }

        internal static async Task SetRandomStatus()
        {
            Status status = StatusConfig.Statuses[_random.Next(0, StatusConfig.Statuses.Count)];
            await SetStatus(status.ActivityType, status.Message);
        }

        internal static async Task SaveConfig()
        {
            try
            {
                using FileStream fs = File.Create(_configFile);
                await JsonSerializer.SerializeAsync(fs, StatusConfig, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving {nameof(StatusModule)} config");
            }
        }

        internal static async Task LoadConfig()
        {
            try
            {
                using FileStream fs = File.OpenRead(_configFile);
                StatusConfig = await JsonSerializer.DeserializeAsync<StatusModuleConfig>(fs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading {nameof(StatusModule)} config");
            }
        }
    }
}
