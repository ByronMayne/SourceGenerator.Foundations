using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using SGF;
using System;
using System.Text;

namespace ConsoleApp.SourceGenerator
{
    [SgfGenerator]
    internal class ConsoleAppSourceGenerator : IncrementalGenerator
    {
        public class Payload
        {
            public string? Name { get; set; }
            public string? Version { get; set; }
        }

        public string WarningMessage { get; set; }

        public ConsoleAppSourceGenerator() : base("ConsoleAppSourceGenerator")
        {
            WarningMessage = "Warnings show up in the 'Build' pane along with the 'Source Generators' pane";
        }

        public override void OnInitialize(SgfInitializationContext context)
        {
            Payload payload = new()
            {
                Name = "Newtonsoft.Json",
                Version = "13.0.1"
            };

            Logger.Warning(WarningMessage);
            Logger.Information("This is the output from the source generator assembly ConsoleApp.SourceGenerator");
            Logger.Information("This generator references Newtonsoft.Json and it can just be referenced without any other boilerplate");
            Logger.Information(JsonConvert.SerializeObject(payload));
            Logger.Information("Having the log makes working with generators much simpler!");

            context.RegisterPostInitializationOutput(AddSource);
        }

        private void AddSource(IncrementalGeneratorPostInitializationContext context)
        {
            SourceText sourceText = SourceText.From("""
                namespace Examples 
                {
                    public class Person
                    {
                        public string Name { get; }

                        public Person(string name)
                        {
                            Name = name;
                        }
                    }
                }
                """, Encoding.UTF8);

            context.AddSource("Person.g.cs", sourceText);
        }

        public override void OnException(Exception exception)
        {

            base.OnException(exception);
        }
    }
}
