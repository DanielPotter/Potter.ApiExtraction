using System;
using System.Collections.Generic;
using System.Reflection;

namespace Potter.ApiExtraction.Core.Tests.Constants
{
    public class ExpectationForAssembly
    {
        public Assembly Assembly { get; set; }

        public ApiElement Configuration { get; set; }

        public List<CompilationUnitExpectation> CompilationUnits { get; set; } = new List<CompilationUnitExpectation>();

        public List<Type> ReferenceTypes { get; set; } = new List<Type>();
    }
}
