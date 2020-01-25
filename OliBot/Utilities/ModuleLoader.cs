using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace OliBot.Utilities
{
    public static class ModuleLoader
    {
        public static void LoadModules()
        {
            string moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), "Modules");

            if (!Directory.Exists(moduleFolder))
                Directory.CreateDirectory(moduleFolder);

            IEnumerable<string> modules = Directory.EnumerateFiles(moduleFolder);

            foreach (string module in modules)
                Assembly.LoadFile(module);
        }
    }
}
