using OliBot.API;
using System;
using NUnit.Framework;
using BotCore.Commands.Models;
using BotCore.Commands.Converters;

namespace BotCore.Tests.Converters
{
    public class EnumConverterTests
    {
        [TestCase(nameof(DIType.Scoped), typeof(DIType))]
        public void WillReturnCorrectType(string input, Type type)
        {
            Assert.IsTrue(EnumConverter.TryParse(type, input, out object parsed));

            Assert.IsTrue(parsed.GetType() == type);
        }

        [Test]
        public void WillFailForIncorrectType()
        {
            Assert.IsFalse(EnumConverter.TryParse(typeof(Command), "String", out _));
        }

        [TestCase("Singleton", true, DIType.Singleton)]
        [TestCase("singleton", true, DIType.Singleton)]
        [TestCase("3", true, DIType.Singleton)]
        [TestCase("0", true, DIType.HostedService)]
        [TestCase("9999", false, DIType.Singleton)]
        [TestCase("", false, DIType.Singleton)]
        public void WillParseCorrectly(string input, bool willConvert, DIType expectedOutput)
        {
            Assert.AreEqual(willConvert, EnumConverter.TryParse(typeof(DIType), input, out object parsed));

            if (willConvert)
                Assert.AreEqual(expectedOutput, (DIType)parsed);
        }
    }
}
