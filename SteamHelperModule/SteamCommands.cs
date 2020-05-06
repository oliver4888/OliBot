using Common;
using Common.Attributes;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.Collections.Generic;

namespace SteamHelperModule
{
    public class SteamCommands
    {
        [Command(permissionLevel: BotPermissionLevel.HostOwner)]
        [Description("Gets stats on the MemoryCache objects used to store Steam data")]
        public async Task CacheStats(CommandContext ctx)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder().WithTitle($"{nameof(SteamHelperModule)} Cache Stats");

            foreach (KeyValuePair<string, SteamItemCache> kvp in SteamWebApiHelper.Caches)
                builder.AddField(kvp.Key, $"{kvp.Value.CacheItemCount} items");

            await ctx.Channel.SendMessageAsync(embed: builder.Build());
        }

        /* Future commands:
         * 
         * Purge All Caches
         * Purge Cache <key>
         * Purge Cache Item <cacheKey> <itemKey>
         */
    }
}
