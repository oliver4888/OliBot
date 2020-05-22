using Common;
using System;
using Common.Attributes;
using Common.Extensions;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SteamHelperModule
{
    public class SteamCommands
    {
        [Command(permissionLevel: BotPermissionLevel.HostOwner, groupName: "Steam")]
        [Alias("SteamStats")]
        [Description("Gets stats on the MemoryCache objects used to store Steam data.")]
        public async Task CacheStats(CommandContext ctx)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle($"{nameof(SteamHelperModule)} Cache Stats");

            if (!ctx.IsDMs)
                builder.WithCustomFooterWithColour(ctx.Message, ctx.Member);

            foreach (KeyValuePair<string, SteamItemCache> kvp in SteamWebApiHelper.Caches)
                builder.AddField(kvp.Key, $"{kvp.Value.CacheItemCount} items");

            await ctx.Channel.SendMessageAsync(embed: builder.Build());

            if (!ctx.IsDMs)
                await ctx.Message.DeleteAsync();
        }

        [Command(permissionLevel: BotPermissionLevel.HostOwner, groupName: "Steam")]
        [Description("Clears all Steam caches.")]
        public async Task PurgeAllCaches(CommandContext ctx)
        {
            foreach (KeyValuePair<string, SteamItemCache> kvp in SteamWebApiHelper.Caches)
                kvp.Value.Clear();

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.BotCoreModule.DiscordClient, ":white_check_mark:"));
        }

        [Command(permissionLevel: BotPermissionLevel.HostOwner, groupName: "Steam")]
        [Description("Clears the specified Steam cache.")]
        public async Task PurgeCache(CommandContext ctx, string cacheName)
        {
            if (!SteamWebApiHelper.Caches.ContainsKey(cacheName))
            {
                await ctx.Message.Channel.SendMessageAsync("Unknown Steam cache, valid Steam caches:" +
                    $"{Environment.NewLine}{string.Join(Environment.NewLine, SteamWebApiHelper.Caches.Keys)}");
                return;
            }

            SteamWebApiHelper.Caches[cacheName].Clear();

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.BotCoreModule.DiscordClient, ":white_check_mark:"));
        }

        [Command(permissionLevel: BotPermissionLevel.HostOwner, groupName: "Steam")]
        [Description("Clears the specified Steam cache.")]
        public async Task PurgeCacheItem(CommandContext ctx, string cacheName, string itemKey)
        {
            if (!SteamWebApiHelper.Caches.ContainsKey(cacheName))
            {
                await ctx.Message.Channel.SendMessageAsync("Unknown Steam cache, valid Steam caches:" +
                    $"{Environment.NewLine}{string.Join(Environment.NewLine, SteamWebApiHelper.Caches.Keys)}");
                return;
            }

            SteamWebApiHelper.Caches[cacheName].RemoveItem(itemKey);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.BotCoreModule.DiscordClient, ":white_check_mark:"));
        }
    }
}
