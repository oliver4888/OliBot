using Common;
using Common.Interfaces;

namespace BotCoreModule.Commands.Converters
{
    public class BoolConverter : IConverter<bool>
    {
        public bool TryParse(string input, CommandContext ctx, out bool parsedValue) => bool.TryParse(input, out parsedValue);
    }
}
