using System;

namespace SourceGenerator.Foundations.Handlebars
{
    /// <summary>
    /// Adds Handlebars to your source generator 
    /// </summary>
    public interface IHandlebars
    {
        /// <summary>
        /// Gets the shared handelbars instance 
        /// </summary>
        IHandlebars Handlebars { get; }
    }
}
