using Common.Interfaces;

namespace BotCoreModule.Commands.Converters
{
    public class BoolConverter : IConverter<bool>
    {
        public bool TryParse(string input, out bool parsedValue) => bool.TryParse(input, out parsedValue);
    }
}
