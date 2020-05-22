namespace BotCoreModule.Commands.Converters
{
    internal interface IConverter<T> : IGenericConverter
    {
        public bool TryParse(string input, out T parsedValue);
    }

    internal interface IGenericConverter { }
}
