using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class CompilationUnitExpectation
    {
        public List<string> Usings { get; set; } = new List<string>();

        public List<NamespaceExpectation> Namespaces { get; set; } = new List<NamespaceExpectation>();
    }
}
