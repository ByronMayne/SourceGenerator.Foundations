using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Collections.Generic;
using System.Diagnostics;
using AssemblyDefinition = Mono.Cecil.AssemblyDefinition;

namespace SourceGenerator.Foundations.Injector
{
    internal class Injector
    {

        private string? PdbFile(string assemblyFile)
        {
            Debug.Assert(assemblyFile != null);
            string path = Path.ChangeExtension(assemblyFile, ".pdb");
            return File.Exists(path) ? path : null;
        }

        public void Inject(string assemblyFile, string typeName, string methodName)
        {
            AssemblyDefinition? assembly = null;

            try
            {
                if (!File.Exists(assemblyFile))
                {
                    throw ExceptionFactory.AssemblyDoesNotExist(assemblyFile);
                }
                assembly = ReadAssembly(assemblyFile);
                MethodReference callee = GetCalleeMethod(typeName, methodName, assembly);
                InjectInitializer(callee, assembly);

                WriteAssembly(assemblyFile, assembly);
            }
            catch (Exception ex)
            {
                throw new InjectionException(ex.Message);
            }
            finally
            {
                assembly?.Dispose();
            }
        }

        private void InjectInitializer(MethodReference callee, AssemblyDefinition assemblyDefinition)
        {
            Debug.Assert(assemblyDefinition != null);
            TypeReference voidRef = assemblyDefinition.MainModule.ImportReference(callee.ReturnType);
            const MethodAttributes attributes = MethodAttributes.Static
                                                | MethodAttributes.SpecialName
                                                | MethodAttributes.RTSpecialName;
            MethodDefinition cctor = new(".cctor", attributes, voidRef);
            ILProcessor il = cctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Call, callee));
            il.Append(il.Create(OpCodes.Ret));

            TypeDefinition? moduleClass = Find(assemblyDefinition.MainModule.Types, t => t.Name == "<Module>");
            if(moduleClass == null)
            {
                throw ExceptionFactory.ModuleNotFound();
            }
            
            // Always insert first so that we appear before [ModuleInitalizerAttribute]
            moduleClass.Methods
                .Insert(0, cctor);

            Debug.Assert(moduleClass != null, "Found no module class!");
        }

        private void WriteAssembly(string assemblyFile, AssemblyDefinition assembly)
        {
            var writeParams = new WriterParameters();
            if (PdbFile(assemblyFile) != null)
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new PdbWriterProvider();
            }
            assembly.Write(assemblyFile, writeParams);
        }

        private AssemblyDefinition ReadAssembly(string assemblyFile)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyFile));

            var readParams = new ReaderParameters(ReadingMode.Immediate)
            {
                AssemblyResolver = resolver,
                InMemory = true
            };

            if (PdbFile(assemblyFile) != null)
            {
                readParams.ReadSymbols = true;
                readParams.SymbolReaderProvider = new PdbReaderProvider();
            }
            return AssemblyDefinition.ReadAssembly(assemblyFile, readParams);
        }

        private MethodReference GetCalleeMethod(string typeName, string methodName, AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.MainModule;
            TypeDefinition? moduleInitializerClass;

            moduleInitializerClass = Find(module.Types, t => t.FullName == typeName);
            if (moduleInitializerClass == null)
            {
                throw ExceptionFactory.TypeNameDoesNotExist(typeName);
            }

            MethodDefinition? callee = Find(moduleInitializerClass.Methods, m => m.Name == methodName);
            if (callee == null)
            {
                throw ExceptionFactory.MethodNameDoesNotExist(moduleInitializerClass.FullName, methodName);
            }
            if (callee.Parameters.Count > 0)
            {
                throw ExceptionFactory.ModuleInitializerMayNotHaveParameters();
            }
            if (callee.IsPrivate || callee.IsFamily)
            {
                throw ExceptionFactory.ModuleInitializerMayNotBePrivate();
            }
            if (!callee.ReturnType.FullName.Equals(typeof(void).FullName)) //Don't compare the types themselves, they might be from different CLR versions.
            {
                throw ExceptionFactory.ModuleInitializerMustBeVoid();
            }
            return !callee.IsStatic ? throw ExceptionFactory.ModuleInitializerMustBeStatic() : callee;
        }

        //No LINQ, since we want to target 2.0
        private static T? Find<T>(Collection<T> objects, Predicate<T> condition) where T : class
        {
            foreach (T obj in objects)
            {
                if (condition(obj))
                {
                    return obj;
                }
            }
            return null;
        }
    }
}
