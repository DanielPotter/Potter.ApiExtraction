using System;
using System.Collections.Generic;
using System.Reflection;
using Potter.ApiExtraction.Core.Configuration;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class ExpectationForAssembly
    {
        public Assembly Assembly { get; set; }

        public ApiConfiguration Configuration { get; set; }

        public List<CompilationUnitExpectation> CompilationUnits { get; set; } = new List<CompilationUnitExpectation>();

        public List<Type> ReferenceTypes { get; set; } = new List<Type>();
    }
}
