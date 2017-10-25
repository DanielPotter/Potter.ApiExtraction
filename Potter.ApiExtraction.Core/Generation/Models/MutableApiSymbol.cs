using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about symbol metadata.
    /// </summary>
    public abstract class MutableApiSymbol : IApiSymbol
    {
        /// <summary>
        ///     Gets the name of the current symbol.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the type that declares this symbol.
        /// </summary>
        public IApiType DeclaringType { get; set; }

        /// <summary>
        ///     Gets a collection that contains this symbol's custom attributes.
        /// </summary>
        public List<IApiAttribute> CustomAttributes { get; set; } = new List<IApiAttribute>();

        IReadOnlyList<IApiAttribute> IApiSymbol.CustomAttributes => CustomAttributes;
    }
}
