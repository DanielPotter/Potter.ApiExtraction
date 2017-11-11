using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class NamespaceExpectation
    {
        public string Namespace { get; set; }

        public List<TypeExpectation> Types { get; set; } = new List<TypeExpectation>();
    }
}
