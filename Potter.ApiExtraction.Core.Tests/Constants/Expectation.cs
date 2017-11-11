using System;
using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class Expectation
    {
        public Type Type { get; set; }

        public CompilationUnitExpectation CompilationUnit { get; set; }

        public List<Type> ReferenceTypes { get; set; }
    }
}
