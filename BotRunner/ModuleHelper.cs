using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Common;

namespace BotRunner
{
    public static class ModuleHelper
    {
        public static void LoadModules()
        {
            string moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), "Modules");

            if (!Directory.Exists(moduleFolder))
                Directory.CreateDirectory(moduleFolder);

            Directory.EnumerateFiles(moduleFolder).Where(file => file.EndsWith("Module.dll")).ToList().ForEach(file => Assembly.LoadFile(file));
        }

        public static IEnumerable<Type> GetModules()
        {
            Type attribute = typeof(ModuleAttribute);
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.Name.EndsWith("Module") && type.IsDefined(attribute, false));
        }
    }
}
