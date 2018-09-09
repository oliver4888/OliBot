﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace discord_bot.Classes
{
    public static class TokenHelper
    {
        private static Dictionary<string, string> _tokens;

        public static async Task LoadTokens(string tokenFile)
        {
            _tokens = new Dictionary<string, string>();

            if (!File.Exists(tokenFile))
            {
                CreateTokensFile(tokenFile);
                return;
            }

            string text = File.ReadAllText(tokenFile);
            XElement rootElement = XElement.Parse(text);
            foreach (var el in rootElement.Elements())
            {
                _tokens.Add(el.Name.LocalName, el.Value);
            }
        }

        private static void CreateTokensFile(string tokenFile)
        {
            _tokens = new Dictionary<string, string>();

            XElement el = new XElement("root", _tokens.Select(kv => new XElement(kv.Key, kv.Value)));

            el.Save(tokenFile);
        }

        public static Dictionary<string, string> GetAllTokens()
        {
            return _tokens;
        }

        public static bool TokensLoaded()
        {
            return _tokens == null;
        }

        public static bool AtLeastOneTokenExists()
        {
            return _tokens.Count > 0;
        }

        public static bool TokenExists(string tokenKey)
        {
            return _tokens.ContainsKey(tokenKey);
        }

        public static string GetTokenValue(string tokenKey)
        {
            if (!TokenExists(tokenKey))
                return null;

            _tokens.TryGetValue(tokenKey, out string value);

            return value;
        }
    }

    [Serializable]
    public struct Token
    {
        public string Name;
        public string Value;

        public Token(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}