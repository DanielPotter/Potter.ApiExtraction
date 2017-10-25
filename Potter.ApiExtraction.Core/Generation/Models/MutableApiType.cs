using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Represents type declarations: class types, interface types, array types, or value types.
    /// </summary>
    public class MutableApiType : MutableApiTypeBase, IApiType
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MutableApiType"/> class.
        /// </summary>
        public MutableApiType()
        {
            Members.CollectionChanged += onMembersChanged;
        }

        private void onMembersChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (IApiMember item in args.NewItems)
            {
                if (item is MutableApiMember member)
                {
                    member.DeclaringType = this;
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating type of member - method, constructor, event, and so on.
        /// </summary>
        public override MemberTypes MemberType => MemberTypes.TypeInfo;

        /// <summary>
        ///     Gets the kind of the current type (class, struct, or interface).
        /// </summary>
        public TypeKind Kind { get; set; }

        /// <summary>
        ///     Gets the type from which the current type directly inherits.
        /// </summary>
        public IApiType BaseType { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the current type has type parameters that have not
        ///     been replaced by specific types.
        /// </summary>
        public bool ContainsGenericParameters => GenericTypeParameters.Count > 0;

        /// <summary>
        ///     Gets the generic type parameters of the current instance.
        /// </summary>
        public List<IApiTypeParameter> GenericTypeParameters { get; set; } = new List<IApiTypeParameter>();

        IReadOnlyList<IApiTypeParameter> IApiType.GenericTypeParameters => GenericTypeParameters;

        /// <summary>
        ///     Gets the interfaces implemented by this interface.
        /// </summary>
        public List<IApiType> ImplementedInterfaces { get; set; } = new List<IApiType>();

        IReadOnlyList<IApiType> IApiType.ImplementedInterfaces => ImplementedInterfaces;

        /// <summary>
        ///     Gets a collection of the members defined by the current type.
        /// </summary>
        public ObservableCollection<IApiMember> Members { get; } = new ObservableCollection<IApiMember>();

        /// <summary>
        ///     Gets a collection of the constructors declared by the current type.
        /// </summary>
        public IEnumerable<IApiContructor> Constructors => Members.Where(member => member.MemberType == MemberTypes.Constructor).Cast<IApiContructor>();

        /// <summary>
        ///     Gets a collection of the indexers defined by the current type.
        /// </summary>
        public IEnumerable<IApiIndexer> Indexers => Members.Where(member => member.MemberType == MemberTypes.Indexer).Cast<IApiIndexer>();

        /// <summary>
        ///     Gets a collection of the properties defined by the current type.
        /// </summary>
        public IEnumerable<IApiProperty> Properties => Members.Where(member => member.MemberType == MemberTypes.Property).Cast<IApiProperty>();

        /// <summary>
        ///     Gets a collection of the nested types defined by the current type.
        /// </summary>
        public IEnumerable<IApiTypeBase> NestedTypes => Members.Where(member => member.MemberType.HasFlag(MemberTypes.TypeInfo)).Cast<IApiTypeBase>();

        /// <summary>
        ///     Gets a collection of the methods defined by the current type.
        /// </summary>
        public IEnumerable<IApiMethod> Methods => Members.Where(member => member.MemberType == MemberTypes.Method).Cast<IApiMethod>();

        IEnumerable<IApiMember> IApiType.Members => Members;

        /// <summary>
        ///     Gets a collection of the fields defined by the current type.
        /// </summary>
        public IEnumerable<IApiEvent> Events => Members.Where(member => member.MemberType == MemberTypes.Event).Cast<IApiEvent>();

        /// <summary>
        ///     Gets a collection of the events defined by the current type.
        /// </summary>
        public IEnumerable<IApiField> Fields => Members.Where(member => member.MemberType == MemberTypes.Field).Cast<IApiField>();
    }
}
