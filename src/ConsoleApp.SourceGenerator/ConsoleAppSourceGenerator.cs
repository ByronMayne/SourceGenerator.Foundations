using SGF;
using Microsoft.CodeAnalysis;
using Serilog;
using Newtonsoft.Json;

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
            Payload payload = new Payload()
            {
                Name = "Newtonsoft.Json",
                Version = "13.0.1"
            };

            Log.Information("This is the output from the sournce generator assembly ConsoleApp.SourceGenerator");
            Log.Information("This generator references Newtonsoft.Json and it can just be referenced without any other boilerplate");
            Log.Information(JsonConvert.SerializeObject(payload));
            Log.Information("Having the log makes working with generators much simpler!");
        }
    }
}
