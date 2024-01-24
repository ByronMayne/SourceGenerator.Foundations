#nullable enable
#pragma warning disable

namespace System.Diagnostics.CodeAnalysis
{
    using global::System;

    /// <summary>
    /// Specifies that the method will not return if the associated <see cref="Boolean"/>
    /// parameter is passed the specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    [ExcludeFromCodeCoverage, DebuggerNonUserCode]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        /// <summary>
        /// Gets the condition parameter value. Code after the method is considered unreachable
        /// by diagnostics if the argument to the associated parameter matches this value.
        /// </summary>
        public bool ParameterValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoesNotReturnIfAttribute"/> 
        /// class with the specified parameter value.
        /// </summary>
        /// <param name="parameterValue">
        /// The condition parameter value. Code after the method is considered 
        /// unreachable by diagnostics if the argument to the associated parameter matches this value.
        /// </param>
        public DoesNotReturnIfAttribute(bool parameterValue)
        {
        }
    }
}

#pragma warning restore
#nullable restore