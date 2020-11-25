namespace OliBot.API.Interfaces
{
    public interface IConverter<T> : IGenericConverter
    {
        public bool TryParse(string input, CommandContext ctx, out T parsedValue);
    }

    public interface IGenericConverter { }
}
