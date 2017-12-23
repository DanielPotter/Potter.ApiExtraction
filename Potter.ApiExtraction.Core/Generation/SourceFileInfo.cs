using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Potter.ApiExtraction.Core.Generation
{
    public class SourceFileInfo
    {
        public SourceFileInfo(string name, string group, string namespaceName, CompilationUnitSyntax compilationUnit)
        {
            Name = name;
            Group = group;
            NamespaceName = namespaceName;
            CompilationUnit = compilationUnit;
        }

        public string Name { get; }

        public string Group { get; }

        public string NamespaceName { get; }

        public CompilationUnitSyntax CompilationUnit { get; }
    }
}
