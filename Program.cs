using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

class Program 
{
    public static int Main(string[] args)
    {
        LoadAllAssemblies();

        return Patcher.Run(args);
    }

    private static void LoadAllAssemblies()
    {
        string[] assemblies =
        {
            "Mono.Cecil.dll",
            "Mono.Cecil.Mdb.dll",
            "Mono.Cecil.Pdb.dll",
            "Mono.Cecil.Rocks.dll",
            "MonoMod.exe",
            "MonoMod.Utils.dll",
            "Mono.Options.dll",
        };

        foreach (var assembly in assemblies)
            LoadAssembly(assembly);
    }

    private static void LoadAssembly(string assembly)
    {
        //Console.WriteLine($"Loading {assembly}...");
        AssemblyLoader.LoadAssembly(assembly);
    }
}
