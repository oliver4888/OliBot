using System;
using System.Linq;

namespace BotCoreModule.Commands.Converters
{
    public static class EnumConverter
    {
        public static bool TryParse(Type type, string value, out object parsedValue)
        {
            parsedValue = null;

            if (!type.IsEnum || string.IsNullOrWhiteSpace(value))
                return false;

            if (int.TryParse(value, out int intValue))
                if (!Enum.IsDefined(type, intValue))
                {
                    return false;
                }
                else
                {
                    parsedValue = Enum.Parse(type, value, true);
                    return true;
                }

            string str = Enum.GetNames(type).FirstOrDefault(x => x.ToLowerInvariant() == value.ToLowerInvariant());

            if (str == null)
                return true;
            else
            {
                parsedValue = Enum.Parse(type, str, true);
                return true;
            }
        }
    }
}
