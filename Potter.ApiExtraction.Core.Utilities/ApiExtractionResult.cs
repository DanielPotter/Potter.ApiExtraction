namespace Potter.ApiExtraction.Core.Utilities
{
    public class ApiExtractionResult
    {
        public ApiExtractionResult(string namespaceName, string name, string sourceCode)
        {
            Namespace = namespaceName;
            Name = name;
            SourceCode = sourceCode;
        }

        public string Namespace { get; }

        public string Name { get; }

        public string SourceCode { get; }
    }
}
