using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Attributes;
using System.Collections.Generic;

namespace BotRunner
{
    public static class ModuleHelper
    {
        public static IEnumerable<Assembly> ModuleAssemblies { get; private set; } = new List<Assembly>();
        public static IEnumerable<Type> ModuleTypes { get; private set; } = new List<Type>();

        public static void LoadModules()
        {
            string moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), "Modules");

            if (!Directory.Exists(moduleFolder))
                Directory.CreateDirectory(moduleFolder);

            ModuleAssemblies = LoadAssemblies(Directory.EnumerateFiles(moduleFolder).Where(file => file.EndsWith("Module.dll")));

            ModuleTypes = ModuleAssemblies.SelectMany(assembly => assembly.GetTypes())
               .Where(type => type.Name.EndsWith("Module") && type.IsDefined(typeof(ModuleAttribute), false));
        }

        static IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> files)
        {
            foreach (string file in files)
                yield return Assembly.LoadFile(file);
        }

    }
}
