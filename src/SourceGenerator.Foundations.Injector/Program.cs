using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;

namespace SourceGenerator.Foundations.Injector
{
    internal class InjectCommand : RootCommand
    {

        public InjectCommand()
        {
            Description = "Takes an assembly and injects a series of methods into the module initalizer that will be called on startup";

            Argument<string> className = new("className", "The full type name of the class that should be added to the moduel initializer");
            Argument<string> methodName = new("methodName", "The name of the static method that should be added to the module initializer");
            Argument<FileInfo> targetAssembly = new Argument<FileInfo>("targetAssembly", "The path to the assembly file that should be injected");
            targetAssembly.ExistingOnly();

            AddArgument(className);
            AddArgument(methodName);
            AddArgument(targetAssembly);

            this.SetHandler(InvokeAsync, className, methodName, targetAssembly);
        }

        private Task<int> InvokeAsync(string className, string methodName, FileInfo targetAssembly)
        {
            Injector injector = new Injector();
            injector.Inject(targetAssembly.FullName, className, methodName);
            return Task.FromResult(0);
        }
    }

    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await new CommandLineBuilder(new InjectCommand())
                .UseHelp()
                .Build()
                .InvokeAsync(args);
        }
    }
}