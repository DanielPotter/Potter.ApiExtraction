using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class TypeExpectation
    {
        public string Declaration { get; set; }

        public List<string> Constraints { get; set; } = new List<string>();

        public List<MemberExpectation> Members { get; set; } = new List<MemberExpectation>();
    }
}
