using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ConsoleApp.SourceGenerator
{
    [Generator]
    internal class ConsoleAppSourceGenerator : IncrementalGenerator
    {
        public class Payload
        {
            public string Name { get; set; }
            public string Version { get; set; }
        }

        public ConsoleAppSourceGenerator() : base()
        {
        }

        public override void Initialize(IncrementalGeneratorInitializationContext context)
        {
            Payload payload = new Payload()
            {
                Name = "Newtonsoft.Json",
                Version = "13.0.1"
            };

            WriteLine("This is the output from the sournce generator assembly ConsoleApp.SourceGenerator");
            WriteLine("This generator references Newtonsoft.Json and it can just be referenced without any other boilerplate");
            WriteLine(JsonConvert.SerializeObject(payload));
            WriteLine("Having the log makes working with generators much simpler!");
        }
    }
}
