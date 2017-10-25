using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about symbol metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the common responsibilities of the
    ///     <see cref="System.Reflection.MemberInfo"/> class.
    /// </remarks>
    public interface IApiSymbol
    {
        /// <summary>
        ///     Gets the name of the current symbol.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type that declares this symbol.
        /// </summary>
        IApiType DeclaringType { get; }

        /// <summary>
        ///     Gets a collection that contains this symbol's custom attributes.
        /// </summary>
        IReadOnlyList<IApiAttribute> CustomAttributes { get; }
    }
}
