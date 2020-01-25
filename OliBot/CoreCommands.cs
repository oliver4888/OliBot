using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Common.Attributes;
using Common.Extensions;

namespace OliBot
{
    public static class CoreCommands
    {
        [Command]
        public static async Task Ping(DiscordClient discord, DiscordMessage message)
        {
            await message.RespondAsync("Pong!");
        }

        [Command]
        public static async Task Stats(DiscordClient discord, DiscordMessage message)
        {
            string uptime = (DateTime.Now - BotCore.StartTime).ToString(@"d\ hh\:mm");
            int guilds = discord.Guilds.Count;
            int users = discord.Guilds.Values.Select(g => g.MemberCount).Sum();
            

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
            {
                Title = discord.CurrentUser.Username
            };
            builder
                .AddField("Uptime", uptime, true)
                .AddField("Guilds", guilds.ToString(), true)
                .AddField("Users", users.ToString(), true);

            await message.RespondWithEmbedAsync(builder);
        }

        [Command]
        public static async Task Test(DiscordClient discord, DiscordMessage message)
        {
            await message.RespondAsync(message.Channel.Guild.Owner.JoinedAt.UtcDateTime.ToString());
        }
    }
}
