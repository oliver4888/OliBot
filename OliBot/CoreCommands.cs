using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Common;

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
            string uptime = (DateTime.Now - BotCore.StartTime).ToString(@"d\.hh\:mm\:ss");
            int guilds = discord.Guilds.Count;
            int members = discord.Guilds.Values.Select(g => g.MemberCount).Sum();
            int uniqueUsers = discord.Guilds.Values.SelectMany(g => g.Members.Values.Select(m => m.Id)).Distinct().Count();

            await message.RespondAsync($"Uptime: {uptime}, Guilds: {guilds}, Members: {members}, Unique Users: {uniqueUsers}");
        }
    }
}
