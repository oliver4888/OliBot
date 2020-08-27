using NUnit.Framework;
using BotCore.Commands.Converters;

namespace BotCore.Tests.Converters
{
    public class BoolConverterTests
    {
        readonly BoolConverter _converter = new BoolConverter();

        [TestCase("true", true, true)]
        [TestCase("false", true, false)]
        [TestCase("TRUE", true, true)]
        [TestCase("RandomText", false, false)]
        public void WillParseCorrectly(string input, bool willConvert, bool expectedOutput)
        {
            Assert.AreEqual(willConvert, _converter.TryParse(input, null, out bool parsed));

            if (willConvert)
                Assert.AreEqual(expectedOutput, parsed);
        }
    }
}