namespace Potter.ApiExtraction.Core.Utilities
{
    public class ApiExtractionResult
    {
        public ApiExtractionResult(string name, string namespaceName, string group, string sourceCode)
        {
            Name = name;
            Namespace = namespaceName;
            Group = group;
            SourceCode = sourceCode;
        }

        public string Name { get; }

        public string Namespace { get; }

        public string Group { get; }

        public string SourceCode { get; }
    }
}
