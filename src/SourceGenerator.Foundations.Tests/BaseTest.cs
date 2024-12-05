using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Xunit.Abstractions;

namespace SourceGenerator.Foundations.Tests
{
    public class BaseTest
    {
        /// <summary>
        /// Gets the name of the currently running test method 
        /// </summary>
        public string TestMethodName { get; }

        protected BaseTest(ITestOutputHelper outputHelper)
        {
            TestMethodName = "Unknown";
            Type type = outputHelper.GetType();
            FieldInfo? testField = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            ITest? test = testField?.GetValue(outputHelper) as ITest;
            if (test != null)
            {
                TestMethodName = test.TestCase.TestMethod.ToString()!;
            }
        }
    }
}