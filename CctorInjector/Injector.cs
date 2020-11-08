using System;
using System.Linq;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace CctorInjector
{
    public class Injector
    {
        private readonly ModuleDef ModuleDef;
        private readonly Type Type;

        private MethodDef Constructor;

        private IEnumerable<IDnlibDef> Members;

        public bool hasInjected;

        public Injector(ModuleDef module, Type type)
        {
            ModuleDef = module;
            Type = type;
        }

        public void Inject()
        {
            try
            {
                Constructor = GetStaticConstructor();
                ModuleDefMD typeModule = ModuleDefMD.Load(Type.Module);
                TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(Type.MetadataToken));
                Members = InjectHelper.Inject(typeDef, ModuleDef.GlobalType, ModuleDef);
                foreach (var md in ModuleDef.GlobalType.Methods)
                {
                    if (md.Name == ".ctor")
                    {
                        ModuleDef.GlobalType.Remove(md);
                        break;
                    }
                }
                hasInjected = true;
            }
            catch (Exception e)
            {
                hasInjected = false;
                Console.WriteLine(e.Message);
            }
        }

        public void CallMethod(string MethodName)
        {
            try
            {
                MethodDef methodDef = (MethodDef)Members.Single(method => method.Name == MethodName);
                Constructor.Body.Instructions.Remove(Constructor.Body.Instructions.First(m => m.OpCode == OpCodes.Ret));
                Constructor.Body.Instructions.Add(new Instruction(OpCodes.Call, methodDef));
                Constructor.Body.Instructions.Add(new Instruction(OpCodes.Ret));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private MethodDef GetStaticConstructor() 
        {
            return ModuleDef.GlobalType.FindOrCreateStaticConstructor();
        }
    }
}