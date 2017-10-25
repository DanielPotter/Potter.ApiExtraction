using System;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Marks each type of member that is defined as an implementation of
    ///     <see cref="IApiMember"/>.
    /// </summary>
    /// <remarks>
    ///     This enumeration estimates the <see cref="System.Reflection.MemberTypes"/> enumeration.
    /// </remarks>
    [Flags]
    public enum MemberTypes
    {
        /// <summary>
        ///     Specifies that the member is a constructor
        /// </summary>
        Constructor = 0x1,

        /// <summary>
        ///     Specifies that the member is an event.
        /// </summary>
        Event = 0x2,

        /// <summary>
        ///     Specifies that the member is a field.
        /// </summary>
        Field = 0x4,

        /// <summary>
        ///     Specifies that the member is a method.
        /// </summary>
        Method = 0x8,

        /// <summary>
        ///     Specifies that the member is a property.
        /// </summary>
        Property = 0x10,

        /// <summary>
        ///     Specifies that the member is a type.
        /// </summary>
        TypeInfo = 0x20,

        /// <summary>
        ///     Specifies that the member is a custom member type.
        /// </summary>
        Custom = 0x40,

        /// <summary>
        ///     Specifies that the member is a nested type.
        /// </summary>
        NestedType = 0x80,

        /// <summary>
        ///     Specifies that the member is a parameter.
        /// </summary>
        Parameter = 0x100,

        /// <summary>
        ///     Specifies that the member is an indexer.
        /// </summary>
        Indexer = 0x200,

        /// <summary>
        ///     Specifies all member types.
        /// </summary>
        All = 0x3BF,
    }
}
