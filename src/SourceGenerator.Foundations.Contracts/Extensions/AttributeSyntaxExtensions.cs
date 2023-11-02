using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SGF.Extensions
{
    /// <summary>
    /// Extensions functions for working with <see cref="AttributeSyntax"/>
    /// </summary>
    internal static class AttributeSyntaxExtensions
    {
        /// <summary>
        /// Attempts to get the value of an attribute based off it's index, returns false if it does not exist
        /// </summary>
        /// <param name="attribute">The current attribute</param>
        /// <param name="index">The expected index</param>
        /// <param name="value">The attributes value if it's found</param>
        /// <returns>True if it was found otherwise false</returns>
        public static bool TryGetArgumentByIndex(this AttributeSyntax attribute, int index, out string? value)
        {
            value = null;

            if (attribute.ArgumentList == null) return false;

            SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList.Arguments;

            if (index >= arguments.Count) return false;

            switch (arguments[index].Expression)
            {
                case LiteralExpressionSyntax literal:
                    if (literal.Token.Value is object rawValue)
                    {
                        value = rawValue.ToString();
                        return true;
                    }
                    return false;
            }
            value = arguments[index].Expression.ToString();
            return true;
        }
    }
}
