using SGF;
using Microsoft.CodeAnalysis;
using Serilog.Core;

namespace ConsoleApp.SourceGenerator
{
    [Generator]
    internal class ConsoleAppSourceGenerator : IncrementalGenerator
    {
        public class Payload
        {
            public string? Name { get; set; }
            public string? Version { get; set; }
        }

        protected override void OnInitialize(IncrementalGeneratorInitializationContext context)
        {
            AttachDebugger();

            Payload payload = new Payload()
            {
                Name = "Newtonsoft.Json",
                Version = "13.0.1"
            };

            Logger.Information("This is the output from the sournce generator assembly ConsoleApp.SourceGenerator");
            Logger.Information("This generator references Newtonsoft.Json and it can just be referenced without any other boilerplate");
            Logger.Information("{@Payload}", payload);
            Logger.Information("Having the log makes working with generators much simpler!");
        }
    }
}
