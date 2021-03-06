﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OliBot.API.Attributes;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BotRunner
{
    public static class ModuleHelper
    {
        internal static IEnumerable<Assembly> ModuleAssemblies { get; private set; } = new List<Assembly>();
        internal static IEnumerable<Type> DependencyInjectedTypes { get; private set; } = new List<Type>();
        internal static IEnumerable<string> PossibleConfigFiles { get; private set; } = new List<string>();

        static IEnumerable<string> PossibleDependencies;

        static string _moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), "Modules");

        internal static void LoadModules(IConfiguration configuration)
        {
            string depFolder = configuration[CommandLineFlags.DebugModuleFolder];

            if (depFolder != null)
            {
                if (Path.IsPathFullyQualified(depFolder))
                    _moduleFolder = depFolder;
                else
                    _moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), depFolder);
            }
            else if (!Directory.Exists(_moduleFolder))
                Directory.CreateDirectory(_moduleFolder);

            AppDomain.CurrentDomain.AssemblyResolve += ResolveMissingDependency;

            IEnumerable<string> dlls = Directory.EnumerateFiles(_moduleFolder);

            PossibleDependencies = dlls.Where(file => file.EndsWith(".dll") && !file.EndsWith("Module.dll"));

            string loadModules = configuration[CommandLineFlags.DebugLoadModules];
            if (loadModules != null)
                dlls = dlls.Concat(loadModules.Split(",")).Distinct();

            ModuleAssemblies = LoadAssemblies(dlls.Where(file => file.EndsWith("Module.dll")));

            string environmentName = EnvironmentHelper.GetEnvironmentName();

            PossibleConfigFiles = ModuleAssemblies.SelectMany(item =>
            {
                string configBaseName = Path.Combine(Path.GetDirectoryName(item.Location), $"appsettings.{Path.GetFileNameWithoutExtension(item.Location)}.");
                return new string[] { configBaseName + "json", configBaseName + environmentName + ".json" };
            });

            string addConfigs = configuration[CommandLineFlags.DebugAddConfigFiles];
            if (addConfigs != null)
                PossibleConfigFiles = PossibleConfigFiles.Concat(addConfigs.Split(",")).Distinct();

            DependencyInjectedTypes = ModuleAssemblies
                .Concat(new List<Assembly> { Assembly.GetExecutingAssembly() })
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsDefined(typeof(DependencyInjectedAttribute), false));
        }

        static Assembly ResolveMissingDependency(object sender, ResolveEventArgs args)
        {
            if (PossibleDependencies == null)
                return null;

            string resolveName = (args.Name.Remove(args.Name.IndexOf(',')) + ".dll").ToLowerInvariant();

            string dep = PossibleDependencies.FirstOrDefault(item => Path.GetFileName(item).ToLowerInvariant() == resolveName);

            return dep == null ? null : Assembly.LoadFile(dep);
        }

        static IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> files)
        {
            foreach (string file in files)
                yield return Assembly.LoadFile(file);
        }
    }
}
