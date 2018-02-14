using System;
using System.Collections.Generic;
using System.Text;

namespace Potter.ApiExtraction.Core.Generation
{
    public class GenerationTypeSource
    {
        public GenerationTypeSource(TypeResolution typeResolution, Type type, IEnumerable<Type> unionTypes = null)
        {
            TypeResolution = typeResolution;
            Type = type;
            UnionTypes = unionTypes;
        }

        public TypeResolution TypeResolution { get; }

        public Type Type { get; }

        public IEnumerable<Type> UnionTypes { get; }
    }
}
