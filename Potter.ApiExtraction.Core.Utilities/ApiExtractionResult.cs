using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Potter.ApiExtraction.Core.Utilities
{
    public class ApiExtractionResult
    {
        public ApiExtractionResult(string namespaceName, string name, string unit)
        {
            Namespace = namespaceName;
            Name = name;
            Unit = unit;
        }

        public string Namespace { get; }

        public string Name { get; }

        public string Unit { get; }
    }
}
