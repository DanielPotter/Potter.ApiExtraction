using System;
using System.Collections.Generic;
using Potter.ApiExtraction.Core.Configuration;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class Expectation
    {
        public Type Type { get; set; }

        public ApiConfiguration Configuration { get; set; }

        public CompilationUnitExpectation CompilationUnit { get; set; }

        public List<Type> ReferenceTypes { get; set; }
    }
}
