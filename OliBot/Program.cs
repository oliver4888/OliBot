using System;
using System.Threading.Tasks;

namespace OliBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new BotCore().Start().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
