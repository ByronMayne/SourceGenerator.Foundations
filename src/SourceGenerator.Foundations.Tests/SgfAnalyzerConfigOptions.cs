using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SGF
{

    public class SgfAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private const string BUILD_PROP_EXTENDED_RULES = "build_property.EnforceExtendedAnalyzerRules";

        private readonly Dictionary<string, string?> m_values;

        public bool EnforceExtendedAnalyzerRules
        {
            get => m_values.TryGetValue(BUILD_PROP_EXTENDED_RULES, out string? rawValue) &&
                bool.TryParse(rawValue, out bool parsedValue) &&
                parsedValue;
            set => m_values[BUILD_PROP_EXTENDED_RULES] = value
                ? "true" // has to be lower 
                : "false";
        }

        public SgfAnalyzerConfigOptions()
        {
            m_values = new Dictionary<string, string?>();

            EnforceExtendedAnalyzerRules = false;
        }

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            if (m_values.TryGetValue(key, out value))
            {
                value ??= "";
                return true;
            }
            return false;
        }
    }
}
