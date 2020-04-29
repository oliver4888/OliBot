using System;
using System.Linq;

namespace BotRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            ModuleHelper.LoadModules();
            ModuleHelper.GetModules().ToList().ForEach(module => Console.WriteLine(module.Name));
            Console.ReadKey();
        }
    }
}
