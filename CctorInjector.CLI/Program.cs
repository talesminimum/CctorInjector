using System;

using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace CctorInjector.CLI
{
    class Program
    {
        static void Main(string[] args) => new Program().Run(args);

        public void Run(string[] args)
        {
            if (args.Length is 0) Environment.Exit(0);
            ModuleDef module = ModuleDefMD.Load(args[0]);

            Injector Injector = new Injector(module, typeof(Test));
            Injector Injector2 = new Injector(module, typeof(Test2));

            Injector.Inject();
            Injector2.Inject();

            Injector.CallMethod("ExecuteMePlease");
            Injector.CallMethod("ExecuteMePlease2");

            Injector2.CallMethod("DontExecuteMe");
            Injector2.CallMethod("DontExecuteMe2");

            SaveFile(module);
            Console.ReadLine();
        }

        public void SaveFile(ModuleDef module)
        {
            var opts = new ModuleWriterOptions(module) { Logger = DummyLogger.NoThrowInstance };
            opts.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            module.Write("Test.exe", opts);
            Console.WriteLine("Module Saved.");
        }
    }

    internal class Test
    {
        public static void ExecuteMePlease() => Console.WriteLine("Thanks!");
        public static void ExecuteMePlease2() => Console.WriteLine("Thanks!");
    }

    internal class Test2
    {
        public static void DontExecuteMe() => Console.WriteLine("Hey! =X");
        public static void DontExecuteMe2() => Console.WriteLine("Hey! =X");
    }
}