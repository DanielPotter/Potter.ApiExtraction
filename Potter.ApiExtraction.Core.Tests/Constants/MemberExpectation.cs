using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class MemberExpectation
    {
        public MemberExpectation(MemberType memberType)
        {
            MemberType = memberType;
        }

        public MemberType MemberType { get; set; }

        public string Declaration { get; set; }

        public List<string> Constraints { get; set; } = new List<string>();
    }
}
