namespace Common.Interfaces
{
    public interface IConverter<T> : IGenericConverter
    {
        public bool TryParse(string input, out T parsedValue);
    }

    public interface IGenericConverter { }
}
