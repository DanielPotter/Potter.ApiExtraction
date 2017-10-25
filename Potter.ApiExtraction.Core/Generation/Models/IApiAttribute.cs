using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to custom attribute data for assemblies, modules, types, members and
    ///     parameters.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.CustomAttributeData"/> class.
    /// </remarks>
    public interface IApiAttribute
    {
        /// <summary>
        ///     Gets the type of the attribute.
        /// </summary>
        IApiType AttributeType { get; }

        /// <summary>
        ///     Gets the list of positional arguments specified for the attribute instance.
        /// </summary>
        IReadOnlyList<IApiAttributeTypedArgument> ConstructorArguments { get; }

        /// <summary>
        ///     Gets the list of named arguments specified for the attribute instance.
        /// </summary>
        IReadOnlyList<IApiAttributeNamedArgument> NamedArguments { get; }
    }
}
