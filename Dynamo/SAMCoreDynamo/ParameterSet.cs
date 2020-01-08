using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMCoreDynamo
{
    public static class ParameterSet
    {
        public static SAM.Core.ParameterSet ByName(string name)
        {
            return new SAM.Core.ParameterSet(name);
        }

        public static object Value(SAM.Core.ParameterSet parameterSet, string name)
        {
            return parameterSet.ToObject(name);
        }
    }
}
