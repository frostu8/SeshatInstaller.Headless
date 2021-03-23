using Mono.Options;
using MonoMod;
using System;
using System.IO;
using System.Collections.Generic;

class Patcher
{
    private string assemblyPath =
        @"C:\Program Files (x86)\Steam\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Managed\Assembly-CSharp.dll";
    private string seshatPath;

    private string AssemblyTmpPath => $"{this.assemblyPath}.tmp";

    private bool helpRequested = false;
    private int verbosity = 0;

    public static int Run(string[] args)
    {
        Patcher program = new Patcher();

        try
        {
            program.Parse(args);
        }
        catch (Exception e)
        {
            PrintUserError(e);
            return 1;
        }

        if (program.helpRequested)
        {
            Console.WriteLine("SeshatInstaller.Headless, a cli tool for patching Library of Ruina.");
            Console.WriteLine("usage: SeshatInstaller.Headless.exe [OPTIONS]+ <path to seshat.dll>");
            Console.WriteLine();

            Console.WriteLine("options:");
            program.Options.WriteOptionDescriptions(Console.Out);
            return 0;
        }

        try
        {
            return program.Patch();
        }
        catch (Exception e)
        {
            PrintException(e);
            Console.WriteLine("A critical error occured!");
            return 1;
        }
    }

    public static void PrintUserError(Exception e)
    {
        Console.Write("SeshatInstaller.Headless: ");
        Console.WriteLine(e.Message);
        Console.WriteLine("Try `SeshatInstaller.Headless.exe --help` for more information.");
    }

    public static void PrintException(Exception e)
    {
        Console.Write(e.GetType().Name);
        Console.Write(": ");
        Console.WriteLine(e.Message);
        Console.WriteLine(e.StackTrace);

        if (e.InnerException != null)
        {
            Console.WriteLine("---------- Start of inner exception ----------");
            PrintException(e.InnerException);
        }
    }


    public int Patch()
    {
        // verify existance of files
        if (!FilesExist())
            return 1;

        // backup target
        BackupTarget();

        Console.WriteLine("Running MonoMod patches...");

        try
        {
            RunMonoMod();
        }
        catch (Exception e)
        {
            PrintException(e);
            Console.WriteLine("MonoMod failed to patch assembly!");
            return 1;
        }

        // copy new file
        CopyResult();

        Console.WriteLine("Done patching~");

        return 0;
    }

    public void CopyResult()
    {
        Console.WriteLine("Writing patch result to disk...");

        File.Copy(this.AssemblyTmpPath, this.assemblyPath, true);
    }

    public void RunMonoMod()
    {
        Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");

        if (this.verbosity > 0)
            Environment.SetEnvironmentVariable("MONOMOD_LOG_VERBOSE", "1");

        using (MonoModder mm = new MonoModder()
        {
            InputPath = this.assemblyPath,
            OutputPath = AssemblyTmpPath,
        })
        {
            // read assembly
            mm.Read();

            // read Seshat.dll
            mm.ReadMod(this.seshatPath);
            mm.MapDependencies();

            // autopatch
            mm.AutoPatch();

            // write assembly
            mm.Write();
        }
    }

    public void BackupTarget()
    {
        string backupAssemblyPath = Path.ChangeExtension(this.assemblyPath, ".vanilla");

        if (File.Exists(backupAssemblyPath))
        {
            // rollback changes instead
            Console.WriteLine("Rolling back changes...");

            File.Copy(backupAssemblyPath, this.assemblyPath, true);

            Console.WriteLine("Rolled back changes.");
        }
        else
        {
            // make a backup
            Console.WriteLine("Making a backup...");

            File.Copy(this.assemblyPath, backupAssemblyPath);

            Console.WriteLine($"Backup created at \"{backupAssemblyPath}\".");
        }
    }

    public bool FilesExist()
    {
        if (!File.Exists(this.assemblyPath))
        {
            Console.WriteLine($"Target assembly \"{this.assemblyPath}\" not found!");
            Console.WriteLine("Specify the path to the Assembly-CSharp.dll with --install-path=");
            return false;
        }

        if (!File.Exists(this.seshatPath))
        {
            Console.WriteLine($"Seshat.dll \"{this.assemblyPath}\" not found!");
            return false;
        }

        return true;
    }

    public void Parse(string[] args)
    {
        List<string> extra = Options.Parse(args);

        if (extra.Count <= 0)
            throw new Exception("Expected path to Seshat.dll");

        this.seshatPath = extra[0];
    }

    #region Options

    public OptionSet Options
    {
        get
        {
            if (_options == null)
                _options = MakeOptions();
            return _options;
        }
    }
    private OptionSet _options;

    private OptionSet MakeOptions()
    {
        return new OptionSet
        {
            { "i|install-path=", "the path to the Assembly-CSharp.dll to patch", p => this.assemblyPath = p },
            { "v", "increases the verbosity level", v => { if (v != null) this.verbosity++; } },
            { "?|h|help", "shows this message and exit", h => this.helpRequested = h != null }
        };
    }

    #endregion Options
}