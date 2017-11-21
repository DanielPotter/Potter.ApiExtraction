using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class TypeExpectation
    {
        [System.Obsolete]
        public TypeExpectation()
            : this(TypeKind.Interface)
        {
        }

        public TypeExpectation(TypeKind kind)
        {
            Kind = kind;
        }

        public TypeKind Kind { get; set; }

        public string Declaration { get; set; }

        public List<string> Constraints { get; set; } = new List<string>();

        public List<MemberExpectation> Members { get; set; } = new List<MemberExpectation>();

        public override string ToString()
        {
            return $"{Declaration} Constraints:({Constraints.Count}) Members:({Members.Count})";
        }
    }
}
