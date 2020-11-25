using DSharpPlus.Entities;

namespace OliBot.API.Extensions
{
    public static class DiscordEmbedBuilderExtensions
    {
        public static DiscordEmbedBuilder WithCustomFooterWithColour(this DiscordEmbedBuilder builder, CommandContext ctx) =>
            builder
                .WithTimestamp(ctx.Message.Id)
                .WithColor(ctx.Member.Color)
                .WithFooter($"{ctx.Member.Username} used {ctx.AliasUsed}", ctx.Member.AvatarUrl);
    }
}
