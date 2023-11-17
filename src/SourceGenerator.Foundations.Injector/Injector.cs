using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Collections.Generic;
using System.Diagnostics;

namespace SourceGenerator.Foundations.Injector
{

    internal class Injector : IDisposable
    {
        private AssemblyDefinition Assembly { get; set; }

        private string? PdbFile(string assemblyFile)
        {
            Debug.Assert(assemblyFile != null);
            string path = Path.ChangeExtension(assemblyFile, ".pdb");
            return File.Exists(path) ? path : null;
        }

        public void Inject(string assemblyFile, string typeName, string methodName)
        {
            try
            {
                if (!File.Exists(assemblyFile))
                {
                    throw ExceptionFactory.AssemblyDoesNotExist(assemblyFile);
                }

                ReadAssembly(assemblyFile);
                MethodReference callee = GetCalleeMethod(typeName, methodName);
                InjectInitializer(callee);

                WriteAssembly(assemblyFile, methodName);
            }
            catch (Exception ex)
            {
                throw new InjectionException(ex.Message);
            }
        }

        private void InjectInitializer(MethodReference callee)
        {
            Debug.Assert(Assembly != null);
            TypeReference voidRef = Assembly.MainModule.ImportReference(callee.ReturnType);
            const MethodAttributes attributes = MethodAttributes.Static
                                                | MethodAttributes.SpecialName
                                                | MethodAttributes.RTSpecialName;
            MethodDefinition cctor = new(".cctor", attributes, voidRef);
            ILProcessor il = cctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Call, callee));
            il.Append(il.Create(OpCodes.Ret));

            TypeDefinition? moduleClass = Find(Assembly.MainModule.Types, t => t.Name == "<Module>");
            if(moduleClass == null)
            {
                throw ExceptionFactory.ModuleNotFound();
            }
            
            // Always insert first so that we appear before [ModuleInitalizerAttribute]
            moduleClass.Methods
                .Insert(0, cctor);

            Debug.Assert(moduleClass != null, "Found no module class!");
        }

        private void WriteAssembly(string assemblyFile, string keyfile)
        {
            Debug.Assert(Assembly != null);
            var writeParams = new WriterParameters();
            if (PdbFile(assemblyFile) != null)
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new PdbWriterProvider();
            }
            Assembly.Write(assemblyFile, writeParams);
        }

        private void ReadAssembly(string assemblyFile)
        {
            Debug.Assert(Assembly == null);

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
            Assembly = AssemblyDefinition.ReadAssembly(assemblyFile, readParams);
        }

        private MethodReference GetCalleeMethod(string typeName, string methodName)
        {
            Debug.Assert(Assembly != null);
            ModuleDefinition module = Assembly.MainModule;
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

        public void Dispose()
        {
            Assembly.Dispose();
        }
    }
}
