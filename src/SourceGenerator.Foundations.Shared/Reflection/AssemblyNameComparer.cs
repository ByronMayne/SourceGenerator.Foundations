using System.Collections.Generic;
using System.Reflection;

namespace SGF.Reflection
{
    /// <summary>
    /// Used to compare two <see cref="AssemblyName"/> to pull them out of the dictionary of types
    /// </summary>
    internal class AssemblyNameComparer : IEqualityComparer<AssemblyName>
    {
        public bool Equals(AssemblyName x, AssemblyName y)
        {
            return string.Equals(GetName(x), GetName(y));
        }

        public int GetHashCode(AssemblyName obj)
        {
            return GetName(obj).GetHashCode();
        }

        private static string GetName(AssemblyName assemblyName)
        {
            string name = assemblyName.Name;
            int index = name.IndexOf(',');
            return index <= 0
                ? name
                : name.Substring(0, index);
        }
    }
}
