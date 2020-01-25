using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace Common.Extensions
{
    public static class DiscordMessageExtensions
    {
        public static async Task RespondWithEmbedAsync(this DiscordMessage message, DiscordEmbed embed) => await message.RespondAsync(embed: embed);
        public static async Task RespondWithEmbedAsync(this DiscordMessage message, DiscordEmbedBuilder builder) => await message.RespondAsync(embed: builder.Build());
    }
}
