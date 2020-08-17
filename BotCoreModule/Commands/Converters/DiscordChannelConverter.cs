using Common;
using Common.Interfaces;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;

namespace BotCoreModule.Commands.Converters
{
    public class DiscordChannelConverter : IConverter<DiscordChannel>
    {
        readonly Regex _channelPattern = new Regex(@"<#(\d+)>");
        public bool TryParse(string input, CommandContext ctx, out DiscordChannel parsedValue)
        {
            parsedValue = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            Match match = _channelPattern.Match(input);

            if (!match.Success || !ulong.TryParse(match.Groups[1].Value, out ulong channelId))
                return false;

            parsedValue = ctx.Guild.GetChannel(channelId);
            return true;
        }
    }
}
