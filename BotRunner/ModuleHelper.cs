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
        public static IEnumerable<Type> DependencyInjectedTypes { get; private set; } = new List<Type>();

        private static IEnumerable<string> PossibleDependencies;

        static readonly string _moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), "Modules");

        public static void LoadModules()
        {
            if (!Directory.Exists(_moduleFolder))
                Directory.CreateDirectory(_moduleFolder);

            AppDomain.CurrentDomain.AssemblyResolve += ResolveMissingDependency;

            IEnumerable<string> dlls = Directory.EnumerateFiles(_moduleFolder);

            PossibleDependencies = dlls.Where(file => file.EndsWith(".dll") && !file.EndsWith("Module.dll"));

            ModuleAssemblies = LoadAssemblies(dlls.Where(file => file.EndsWith("Module.dll")));

            DependencyInjectedTypes = ModuleAssemblies
                .Concat(new List<Assembly> { Assembly.GetExecutingAssembly() })
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsDefined(typeof(DependencyInjectedAttribute), false));
        }

        private static Assembly ResolveMissingDependency(object sender, ResolveEventArgs args)
        {
            if (PossibleDependencies == null)
                return null;

            string dep = PossibleDependencies.FirstOrDefault(item => item == Path.Combine(_moduleFolder, args.Name.Remove(args.Name.IndexOf(',')) + ".dll"));
            if (dep == null)
                return null;
            else
                return Assembly.LoadFile(dep);
        }

        static IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> files)
        {
            foreach (string file in files)
                yield return Assembly.LoadFile(file);
        }

    }
}
