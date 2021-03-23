using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.IO.Compression;
using System.IO;
using System.Reflection;

static class AssemblyLoader
{
    public static string LoaderPath
        //=> Path.Combine(Path.GetTempPath(), @"SeshatInstaller.Headless.libs\");
        => AppDomain.CurrentDomain.BaseDirectory;

    private static ResourceManager GetResourceManager()
        => new ResourceManager("Resources", typeof(AssemblyLoader).Assembly);

    public static void LoadAssembly(string assembly)
    {
        string assemblyPath = Path.Combine(LoaderPath, assembly);

        if (!Directory.Exists(LoaderPath))
        {
            // make loader path
            DirectoryInfo loaderDir = new DirectoryInfo(LoaderPath);
            loaderDir.Create();
            loaderDir.Attributes |= FileAttributes.Hidden;
        }

        if (!File.Exists(assemblyPath))
            // copy file from resources
            UnpackAssembly(assembly);

        Assembly.LoadFrom(assemblyPath);
    }

    public static void UnpackAssembly(string assembly)
    {
        string assemblyPath = Path.Combine(LoaderPath, assembly);

        using (FileStream file = File.OpenWrite(assemblyPath))
            LoadGZipCompressed(assembly, file);
    }

    public static void LoadGZipCompressed(string path, Stream stream)
    {
        using (Stream resource = GetResourceManager().GetStream(path))
        {
            using (GZipStream compressed = new GZipStream(resource, CompressionMode.Decompress))
            {
                compressed.CopyTo(stream);
            }
        }
    }
}
