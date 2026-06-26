using Microsoft.CodeAnalysis;
using SGF.Analyzer.Rules;
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

        /// <summary>
        /// Asserts that the diagnostics contain a diagnostic for the specified rule.
        /// </summary>
        /// <typeparam name="TRule">The rule to check for</typeparam>
        /// <returns></returns>
        public static Action<ImmutableArray<Diagnostic>> TriggersRule<TRule>() where TRule : AnalyzerRule
        {
            string descriptorId = AnalyzerRule.GetDescriptorId<TRule>();

            return diagnostics =>
            {
                if (diagnostics.Length == 0)
                {
                    Assert.Fail($"Expected diagnostic {descriptorId} but no diagnostics were emitted");
                }

                if (diagnostics.Any(d => d.Descriptor.Id == descriptorId))
                {
                    return;
                }

                Assert.Fail($"Expected diagnostic with ID '{descriptorId}' was not found in the emitted diagnostics. The emitted ones were ${string.Join(", ", diagnostics.Select(d => d.Descriptor.Id))}");
            };
        }

        public static Action<ImmutableArray<Diagnostic>> NoneOfSeverity(params DiagnosticSeverity[] severity)
        {
            HashSet<DiagnosticSeverity> set = new HashSet<DiagnosticSeverity>(severity);

            return diagnostics =>
            {
                List<Diagnostic> filtered = diagnostics
                    .Where(d => set.Contains(d.Severity))
                    .ToList();

                if (filtered.Count > 0)
                {
                    StringBuilder errorBuilder = new StringBuilder();

                    foreach (var dia in filtered)
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
