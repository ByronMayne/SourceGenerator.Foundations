using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Configuration
{
    internal class ResourceConfiguration
    {
        /// <summary>
        /// Gets the prefix that is added to all embedded assembly resource
        /// </summary>
        internal static string AssemblyResourcePrefix { get; }

        /// <summary>
        /// Gets the prefix that is added to all embedded script resources
        /// </summary>
        internal static string ScriptPrefix { get; }
    
        static ResourceConfiguration()
        {
            AssemblyResourcePrefix = "SGF.Assembly::";
            ScriptPrefix = "SGF.Script::";
        }
    }
}
