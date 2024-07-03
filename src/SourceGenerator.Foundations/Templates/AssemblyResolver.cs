using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace SGF.Templates;
public static class AssemblyResolverTemplate
{
    public static SourceText Render(string @namespace) => SourceText.From($$"""
#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace {{@namespace}}
{
    /// <summary>
    /// Used to compare two <see cref="AssemblyName"/> to pull them out of the dictionary of types
    /// </summary>
    internal class AssemblyNameComparer : IEqualityComparer<AssemblyName>
    {
        public bool Equals(AssemblyName? x, AssemblyName? y)
        {
            bool isLhsNull = ReferenceEquals(x, null);
            bool isRhsNull = ReferenceEquals(y, null);

            if(isLhsNull && isRhsNull)
            {
                return true;
            }

            if(isLhsNull != isRhsNull)
            {
                return false;
            }

            return string.Equals(GetName(x!), GetName(y!));
        }

        public int GetHashCode(AssemblyName obj)
        {
            return GetName(obj).GetHashCode();
        }

        private static string GetName(AssemblyName assemblyName)
        {
            string name = assemblyName.FullName;
            int index = name.IndexOf(',');
            return index <= 0
                ? name
                : name.Substring(0, index);
        }
    }

  
}
""", Encoding.UTF8);
}