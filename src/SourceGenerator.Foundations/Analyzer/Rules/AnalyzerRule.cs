using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

namespace SGF.Analyzer.Rules
{
    internal abstract class AnalyzerRule
    {
        protected readonly static Dictionary<Type, string> s_ruleToDescriptorMap;


        /// <summary>
        /// Gets the descriptor that this rule creates
        /// </summary>
        public DiagnosticDescriptor Descriptor { get; }

        /// <summary>
        /// Gets the current context 
        /// </summary>
        protected SyntaxNodeAnalysisContext Context { get; private set; }

        static AnalyzerRule()
        {
            s_ruleToDescriptorMap = new Dictionary<Type, string>();
        }

        public AnalyzerRule(DiagnosticDescriptor descriptor)
        {
            Descriptor = descriptor;
            s_ruleToDescriptorMap[GetType()] = descriptor.Id;
        }

        /// <summary>
        /// Invokes the rule 
        /// </summary>
        public void Invoke(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
        {
            Context = context;
            try
            {
                Analyze(classDeclaration);
            }
            finally
            {
                Context = default;
            }
        }


        /// <summary>
        /// Gets the rule id for the given rule type
        /// </summary>
        /// <typeparam name="T">The type of the rule</typeparam>
        /// <returns>The descriptor id for the given rule type</returns>
        public static string GetDescriptorId<T>() where T : AnalyzerRule
        {
            return s_ruleToDescriptorMap[typeof(T)];
        }

        /// <summary>
        /// Tells the rule to analyze and report and errors that it sees
        /// </summary>
        protected abstract void Analyze(ClassDeclarationSyntax classDeclaration);

        /// <summary>
        /// Creates new <see cref="Diagnostic"/> using the <see cref="Descriptor"/> and reports
        /// it to the current context
        /// </summary>
        /// <param name="location">The location to put the diagnostic</param>
        /// <param name="messageArgs">Arguments that are used for it</param>
        protected void ReportDiagnostic(Location location, params object[] messageArgs)
        {
            Diagnostic diagnostic = Diagnostic.Create(Descriptor, location, messageArgs); ;
            Context.ReportDiagnostic(diagnostic);
        }

        protected bool TryGetAttribute(ClassDeclarationSyntax classDeclaration, string name, out AttributeSyntax? attribute)
            => TryGetAttribute(classDeclaration, name, StringComparison.Ordinal, out attribute);

        protected bool TryGetAttribute(ClassDeclarationSyntax classDeclaration, string name, StringComparison stringComparison, out AttributeSyntax? attribute)
        {
            attribute = GetAttribute(classDeclaration, name);
            return attribute != null;
        }

        protected AttributeSyntax? GetAttribute(ClassDeclarationSyntax classDeclarationSyntax, string name, StringComparison stringComparison = StringComparison.Ordinal)
        {
            const string POSTFIX = "Attribute";

            string alternativeName = name.EndsWith(POSTFIX, StringComparison.Ordinal)
                ? name.Substring(0, name.Length - POSTFIX.Length)
                : $"{name}{POSTFIX}";

            foreach (AttributeListSyntax attributeList in classDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    string attributeName = attribute.Name.ToString();

                    if (string.Equals(attributeName, name, stringComparison) ||
                       string.Equals(attributeName, alternativeName, stringComparison))
                    {
                        return attribute;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns back if the attribute with the given name is applied to the type
        /// </summary>
        protected bool HasAttribute(ClassDeclarationSyntax classDeclarationSyntax, string name, StringComparison stringComparison = StringComparison.Ordinal)
            => GetAttribute(classDeclarationSyntax, name, stringComparison) != null;
    }
}
