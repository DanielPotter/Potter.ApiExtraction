using System;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Defines the modifiers that change how a member may be extended.
    /// </summary>
    [Flags]
    public enum ExtensionModifers
    {
        /// <summary>
        ///     The member follows the default extension behavior.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The member may not be extended.
        /// </summary>
        Sealed = 0x1,

        /// <summary>
        ///     The member must be extended.
        /// </summary>
        Abstract = 0x2,

        /// <summary>
        ///     The member may be extended.
        /// </summary>
        Virtual = 0x4,

        /// <summary>
        ///     The member extends another member.
        /// </summary>
        Override = 0x8,

        /// <summary>
        ///     The member hides another member.
        /// </summary>
        New = 0x10,

        /// <summary>
        ///     The member has no extension capability.
        /// </summary>
        Static = Sealed | Abstract,
    }
}
