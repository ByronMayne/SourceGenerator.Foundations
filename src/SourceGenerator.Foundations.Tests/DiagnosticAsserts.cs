using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace SourceGenerator.Foundations.Tests
{
    /// <summary>
    /// Contains asserts for checking if diagnostics match expected values.
    /// </summary>
    internal static class DiagnosticAsserts
    {
        public static Action<ImmutableArray<Diagnostic>> NoErrors()
            => NoneOfSeverity(DiagnosticSeverity.Error);

        public static Action<ImmutableArray<Diagnostic>> NoErrorsOrWarnings()
            => NoneOfSeverity(DiagnosticSeverity.Warning, DiagnosticSeverity.Error);

        public static Action<ImmutableArray<Diagnostic>> NoneOfSeverity(params DiagnosticSeverity[] severity)
        {
            HashSet<DiagnosticSeverity> set = new HashSet<DiagnosticSeverity>(severity);

            return diagnostics =>
            {
                List<Diagnostic> filtered = diagnostics
                    .Where(d => set.Contains(d.Severity))
                    .ToList();

                if(filtered.Count > 0)
                {
                    StringBuilder errorBuilder = new StringBuilder();

                    foreach(var dia in filtered)
                    {
                        Location location = dia.Location;
                        string? filePath = Path.GetFileName(location.SourceTree?.FilePath);

                        string error = $"{filePath} {location.SourceSpan.Start} {dia.Severity} {dia.Descriptor.Id}: {dia.GetMessage()}";

                        errorBuilder.AppendLine(error);
                    }
                    Assert.Fail(errorBuilder.ToString());
                }
            };
        }
    }
}
