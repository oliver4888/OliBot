using System;
using NUnit.Framework;
using BotCore.Commands.Converters;

namespace BotCore.Tests.Converters
{
    public class UriConverterTests
    {
        readonly UriConverter _converter = new UriConverter();

        [TestCase("", false, "")]
        [TestCase("https://oliver4888.com", true, "https://oliver4888.com/")]
        public void WillParseCorrectly(string input, bool willConvert, string expectedOutput)
        {
            Assert.AreEqual(willConvert, _converter.TryParse(input, null, out Uri result));

            if (willConvert)
                // I'm not sure of a better way to compare since attribute arguments must be constant
                Assert.AreEqual(expectedOutput, result.AbsoluteUri);
        }

        [TestCase("file://C:/path/file.xml")]
        [TestCase("\\\\serverName\\path\\to\\file.css")]
        [TestCase("C:/path/to/file.txt")]
        public void WillRejectLocalPaths(string input)
        {
            Assert.IsFalse(_converter.TryParse(input, null, out _));
        }
    }
}
