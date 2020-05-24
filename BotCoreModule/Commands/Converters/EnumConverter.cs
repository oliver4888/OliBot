using System;

namespace BotCoreModule.Commands.Converters
{
    public static class EnumConverter
    {
        public static bool TryParse(Type type, string value, out object parsedValue)
        {
            parsedValue = null;
            if (!type.IsEnum || !Enum.IsDefined(type, int.TryParse(value, out int intValue) ? (object)intValue : value))
                return false;

            parsedValue = Enum.Parse(type, value, true);
            return true;
        }
    }
}
