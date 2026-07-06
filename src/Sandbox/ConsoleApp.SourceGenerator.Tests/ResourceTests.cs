using ConsoleApp.SourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SGF
{
    public class ResourceTests
    {
        [Theory]
        [InlineData("SGF.Assembly::Microsoft.CodeAnalysis.dll")]
        [InlineData("SGF.Assembly::Microsoft.CodeAnalysis.CSharp.dll")]
        public void Assembly_Does_Not_Contain_Resource(string resourceName)
        {
            Assembly assembly = typeof(ConsoleAppSourceGeneratorHoist).Assembly;
            string[] resourceNames = assembly.GetManifestResourceNames();
            Assert.DoesNotContain(resourceName, resourceNames);
        }
    }
}
