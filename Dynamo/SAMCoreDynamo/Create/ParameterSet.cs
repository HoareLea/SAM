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
    }
}
