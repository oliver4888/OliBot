using OliBot.API;
using OliBot.API.Interfaces;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;

namespace BotCore.Commands.Converters
{
    public class DiscordRoleConverter : IConverter<DiscordRole>
    {
        readonly Regex _rolePattern = new Regex(@"<@&(\d+)>");

        public bool TryParse(string input, CommandContext ctx, out DiscordRole parsedValue)
        {
            parsedValue = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            Match match = _rolePattern.Match(input);

            if (!match.Success || !ulong.TryParse(match.Groups[1].Value, out ulong roleId))
                return false;

            parsedValue = ctx.Guild.GetRole(roleId);
            return true;
        }
    }
}
