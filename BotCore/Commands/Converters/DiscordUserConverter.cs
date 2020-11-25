using OliBot.API;
using OliBot.API.Interfaces;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;

namespace BotCore.Commands.Converters
{
    public class DiscordUserConverter : IConverter<DiscordUser>
    {
        readonly Regex _userPattern = new Regex(@"<@!?(\d+)>");
        public bool TryParse(string input, CommandContext ctx, out DiscordUser parsedValue)
        {
            parsedValue = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            Match match = _userPattern.Match(input);

            if (!match.Success || !ulong.TryParse(match.Groups[1].Value, out ulong userId))
                return false;

            parsedValue = ctx.BotCoreModule.DiscordClient.GetUserAsync(userId).Result;
            return true;
        }
    }
}
