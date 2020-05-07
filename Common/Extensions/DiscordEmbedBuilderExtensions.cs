using DSharpPlus.Entities;

namespace Common.Extensions
{
    public static class DiscordEmbedBuilderExtensions
    {
        public static DiscordEmbedBuilder WithCustomFooterWithColour(this DiscordEmbedBuilder builder, DiscordMessage message, DiscordMember member) =>
            builder
                .WithTimestamp(message.Id)
                .WithColor(member.Color)
                .WithFooter($"{member.Username} used {message.Content.Split(' ')[0].ToLowerInvariant()}", member.AvatarUrl);
    }
}
