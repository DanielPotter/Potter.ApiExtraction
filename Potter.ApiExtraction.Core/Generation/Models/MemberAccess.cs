using System;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Defines the possible access types for a member.
    /// </summary>
    /// <remarks>
    ///     This enumeration estimates the access responsibilities of the
    ///     <see cref="System.Reflection.MethodAttributes"/> enumeration.
    /// </remarks>
    [Flags]
    public enum MemberAccess
    {
        /// <summary>
        ///     Access is undefined.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Access to the member is restricted to other members of the class itself.
        /// </summary>
        Private = 0x1,

        /// <summary>
        ///     Access to the member is restricted to members of the class and its derived classes.
        /// </summary>
        Protected = 0x2,

        /// <summary>
        ///     Access to the member is unrestricted.
        /// </summary>
        Public = 0x4,

        /// <summary>
        ///     Access to the member is restricted to any class of the assembly.
        /// </summary>
        Internal = 0x8,
    }
}
