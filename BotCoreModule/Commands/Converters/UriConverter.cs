using Common;
using System;
using Common.Interfaces;

namespace BotCoreModule.Commands.Converters
{
    public class UriConverter : IConverter<Uri>
    {
        public bool TryParse(string input, CommandContext ctx, out Uri parsedValue)
        {
            parsedValue = null;

            if (!Uri.IsWellFormedUriString(input, UriKind.Absolute))
                return false;

            Uri uri = new Uri(input);

            if (uri.IsFile)
                return false;

            parsedValue = uri;
            return true;
        }
    }
}
