namespace BotCoreModule.Commands.Converters
{
    // Floating-point types
    public class FloatConverter : IConverter<float>
    {
        public bool TryParse(string input, out float parsedValue) => float.TryParse(input, out parsedValue);
    }

    public class DoubleConverter : IConverter<double>
    {
        public bool TryParse(string input, out double parsedValue) => double.TryParse(input, out parsedValue);
    }

    public class DecimalConverter : IConverter<decimal>
    {
        public bool TryParse(string input, out decimal parsedValue) => decimal.TryParse(input, out parsedValue);
    }

    // Integral types
    public class SByteConverter : IConverter<sbyte>
    {
        public bool TryParse(string input, out sbyte parsedValue) => sbyte.TryParse(input, out parsedValue);
    }

    public class ByteConverter : IConverter<byte>
    {
        public bool TryParse(string input, out byte parsedValue) => byte.TryParse(input, out parsedValue);
    }

    public class ShortConverter : IConverter<short>
    {
        public bool TryParse(string input, out short parsedValue) => short.TryParse(input, out parsedValue);
    }

    public class UShortConverter : IConverter<ushort>
    {
        public bool TryParse(string input, out ushort parsedValue) => ushort.TryParse(input, out parsedValue);
    }

    public class IntConverter : IConverter<int>
    {
        public bool TryParse(string input, out int parsedValue) => int.TryParse(input, out parsedValue);
    }

    public class UIntConverter : IConverter<uint>
    {
        public bool TryParse(string input, out uint parsedValue) => uint.TryParse(input, out parsedValue);
    }

    public class LongConverter : IConverter<long>
    {
        public bool TryParse(string input, out long parsedValue) => long.TryParse(input, out parsedValue);
    }

    public class ULongConverter : IConverter<ulong>
    {
        public bool TryParse(string input, out ulong parsedValue) => ulong.TryParse(input, out parsedValue);
    }
}
