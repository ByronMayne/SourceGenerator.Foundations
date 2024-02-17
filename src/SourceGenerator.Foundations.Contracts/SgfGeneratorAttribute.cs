using System;

namespace SGF
{
    /// <summary>
    /// Applied a class the inheirts from <see cref="IncrementalGenerator"/>
    /// that will have Source Generator Foundations wrapper generated around it. This adds
    /// better error handling and logging to the given generator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SgfGeneratorAttribute : Attribute
    {
    }
}
