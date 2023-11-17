namespace SourceGenerator.Foundations.Injector
{
    internal static class ExceptionFactory
    {
        internal static InjectionException AssemblyDoesNotExist(string assembly)
        {
             return new InjectionException($"Assembly '{assembly}' does not exist");
        }

        internal static InjectionException NoModuleInitializerTypeFound()
        {
             return new InjectionException("Found no type named 'ModuleInitializer', this type must exist or the ModuleInitializer parameter must be used");
        }

        internal static InjectionException InvalidFormatForModuleInitializer()
        {
             return new InjectionException("Invalid format for ModuleInitializer parameter, use Full.Type.Name::MethodName");
        }

        internal static InjectionException TypeNameDoesNotExist(string typeName)
        {
             return new InjectionException($"No type named '{typeName}' exists in the given assembly!");
        }

        internal static InjectionException MethodNameDoesNotExist(string typeName, string methodName)
        {
             return new InjectionException($"No method named '{methodName}' exists in the type '{methodName}'");
        }

        internal static InjectionException ModuleInitializerMayNotBePrivate()
        {
             return new InjectionException("Module initializer method may not be private or protected, use public or internal instead");
        }

        internal static InjectionException ModuleInitializerMustBeVoid()
        {
             return new InjectionException("Module initializer method must have 'void' as  return new InjectionException(type");
        }

        internal static InjectionException ModuleInitializerMayNotHaveParameters()
        {
             return new InjectionException("Module initializer method must not have any parameters");
        }

        internal static InjectionException ModuleInitializerMustBeStatic()
        {
            return new InjectionException("Module initializer method must be static");
        }

        internal static Exception ModuleNotFound()
        {
           return new InjectionException("Unable to find main module in assembly");
        }
    }
}
